using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech;
using Discord.Audio;
using System.IO;
using System.Threading;
using MikuBot.Services;
using System.Speech.AudioFormat;
using System.Collections.Concurrent;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Converter;
using CliWrap;
using YouTubeSearch;
using YoutubeExplode.Videos;

namespace MikuBot.Modules.Voice
{
    class Voice : ModuleBase
    {
        private struct VoiceChannel
        {
            public IVoiceChannel Channel { get; set; }
            public IAudioClient AudioClient { get; set; }
            public VoiceChannel(IVoiceChannel channel, IAudioClient client)
            {
                Channel = channel;
                AudioClient = client;
            }
        }

        private static DiscordSocketClient _client;
        private static Dictionary<ulong, VoiceChannel> _channels;
        private static AudioOutStream _currentStream = null;
        private static ConcurrentDictionary<ulong, CancellationTokenSource> cancel;
        private static ICommandContext _context;

        static Voice()
        {
            _channels = new Dictionary<ulong, VoiceChannel>();
        }

        public Voice(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
            cancel = new ConcurrentDictionary<ulong, CancellationTokenSource>();
        }

        public static IVoiceChannel GetChannel(ulong guildId)
        {
            return _channels[guildId].Channel;
        }

        [Command("join", RunMode=RunMode.Async)]
        public async Task JoinChannel(IVoiceChannel channel = null)
        {
            channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await ReplyAsync("please join a channel first lol");
                return;
            }

            IAudioClient audioClient = null;
            if (_channels.ContainsKey(Context.Guild.Id))
            {
                if (_channels[Context.Guild.Id].Channel == channel) return;

            }else audioClient = await channel.ConnectAsync();
                
            VoiceChannel voiceChannel = new VoiceChannel(channel, audioClient);
            _channels[Context.Guild.Id] = voiceChannel;

            using (var ffmpeg = FFmpeg.CreateStream(@"Media/voiceClips/sup.mp3"))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = audioClient.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                catch(Exception e) { Console.WriteLine(e);}
                finally { await discord.FlushAsync(); }
            }

            audioClient.StreamCreated += async (s, e) => StartListenService(s, e);

            _context = Context;
        }

        [Command("leave", RunMode=RunMode.Async)]
        public async Task LeaveChannel()
        {
            if (!_channels.ContainsKey(Context.Guild.Id))
                return;
            await _channels[Context.Guild.Id].Channel.DisconnectAsync();
            _channels.Remove(Context.Guild.Id);
        }

        [Command("play", RunMode=RunMode.Async)]
        public async Task Play([Remainder]string searchCriteria)
        {
            Console.WriteLine("mhm");
            VideoSearch items = new VideoSearch();
            VideoSearchComponents video = items.GetVideos(searchCriteria, 1).Result[0];
            string url = video.getUrl();
            string thumbnail = video.getThumbnail();
            string title = video.getTitle();
            YoutubeClient youtube = new YoutubeClient();
            StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
            IStreamInfo streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            if (!Queue.QueueExists(Context.Guild.Id)){
                Console.WriteLine("bruh");
                Queue queue = new Queue(Context.Guild.Id);
                queue.AddStream(streamInfo);
            }
            else Queue.GetQueue(Context.Guild.Id).AddStream(streamInfo);

            if (!_channels.ContainsKey(Context.Guild.Id)) await JoinChannel();

            while (!Queue.GetQueue(Context.Guild.Id).IsEmpty())
            {
                MemoryStream memoryStream = new MemoryStream();
                Stream stream = null;
                try
                {
                    stream = await youtube.Videos.Streams.GetAsync(Queue.GetQueue(Context.Guild.Id).GetNextStream());
                }catch{ await Context.Message.ReplyAsync("something went wrong :/");  }

                bool songPlaying = Queue.GetQueue(Context.Guild.Id).GetCurrentStream() != null;

                // song info embed
                EmbedBuilder embed = new EmbedBuilder();
                if (songPlaying) {
                    int queueCount = Queue.GetQueue(Context.Guild.Id).GetCount();
                    embed.Title = "song added to queue!";
                    embed.ThumbnailUrl = thumbnail;
                    embed.AddField("title", title);
                    embed.AddField("position", $"{queueCount}/{queueCount} in queue");
                }
                else
                {
                    embed.Title = title;
                    embed.ThumbnailUrl = thumbnail;
                    embed.Description = "♬♫♪◖(●。●)◗♪♫♬";
                    embed.AddField("channel", $"playing in {_channels[Context.Guild.Id].Channel.Name}!");
                }

                embed.WithColor(Color.Blue);

                var message = await ReplyAsync("", false, embed.Build());

                if (songPlaying) return;

                try
                {
                    await Cli.Wrap("ffmpeg")
                        .WithArguments(" -hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1")
                        .WithStandardInputPipe(PipeSource.FromStream(stream))
                        .WithStandardOutputPipe(PipeTarget.ToStream(memoryStream))
                        .ExecuteAsync();
                } catch (Exception e) { Console.WriteLine(e.ToString()); }

                using (var discord = _channels[Context.Guild.Id].AudioClient.CreatePCMStream(AudioApplication.Mixed))
                {
                    Queue.GetQueue(Context.Guild.Id).SetCurrentStream();
                    try { await discord.WriteAsync(memoryStream.ToArray(), 0, (int)memoryStream.Length); }
                    finally
                    {
                        await message.DeleteAsync();
                        Queue.GetQueue(Context.Guild.Id).Dequeue();
                        Queue.GetQueue(Context.Guild.Id).SetCurrentStreamNull();
                    }
                }
            }
        }

        private static async Task StartListenService(ulong user, AudioInStream inStream)
        {
            var source = new CancellationTokenSource();
            if(cancel.TryAdd(user, source))
            {
                var queue = new Queue<RTPFrame>();
                var timer = new Timer(e =>
                {
                    if (!source.IsCancellationRequested)
                        ProcessVoiceAsync(user, queue.ToArray()).ConfigureAwait(false);

                    queue.Clear();
                }, null, Timeout.Infinite, Timeout.Infinite);

                while(!source.IsCancellationRequested)
                    try
                    {
                        queue.Enqueue(await inStream.ReadFrameAsync(source.Token));
                        timer.Change(125, 0);
                    }
                    catch { }
            }
        }

        private static async Task ProcessVoiceAsync(ulong userId, RTPFrame[] frames)
        {
            RecognizeCompletedEventArgs args;

            using(var stream = new MemoryStream())
            {
                for (int i = 0; i < frames.Length; i++)
                    await stream.WriteAsync(frames[i].Payload, 0, frames[i].Payload.Length);

                stream.Position = 0;

                var recognizeWaiter = new TaskCompletionSource<RecognizeCompletedEventArgs>();
                using (var engine = await SpeechEngine.Get((s, e) => recognizeWaiter.SetResult(e)))
                {
                    engine.Recognize(stream);
                    args = await recognizeWaiter.Task;
                }

                new VoiceCommandHandler(_client, args.Result.Text, _context, _client.GetUser(userId));
            }
        }
    }
}
