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
        private static DiscordSocketClient _client;
        private static IVoiceChannel _channel = null;
        private static IAudioClient _audioClient = null;
        private static AudioOutStream _currentStream = null;
        private static ConcurrentDictionary<ulong, CancellationTokenSource> cancel = new ConcurrentDictionary<ulong, CancellationTokenSource>();
        private static ICommandContext _context;

        private Queue _currentQueue = null;

        public Voice(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
        }

        public static IVoiceChannel GetChannel()
        {
            return _channel;
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

            _channel = channel;

            var audioClient = await channel.ConnectAsync();
            _audioClient = audioClient;

            using(var ffmpeg = FFmpeg.CreateStream(@"Media\voiceClips\sup.mp3"))
            using(var output = ffmpeg.StandardOutput.BaseStream)
            using(var discord = audioClient.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
            }

            audioClient.StreamCreated += async (s, e) => StartListenService(s, e);

            _context = Context;
        }

        [Command("leave", RunMode=RunMode.Async)]
        public async Task LeaveChannel()
        {
            if (_channel == null) return;
            _currentQueue = null;
            Queue.SetCurrentQueue(_currentQueue);
            await _channel.DisconnectAsync();
        }

        [Command("play", RunMode=RunMode.Async)]
        public async Task Play([Remainder]string searchCriteria)
        {
            var items = new VideoSearch();
            var video = items.GetVideos(searchCriteria, 1).Result[0];
            var url = video.getUrl();
            var thumbnail = video.getThumbnail();
            var title = video.getTitle();
            var youtube = new YoutubeClient();
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            if (Queue.GetCurrentQueue() == null) {
                _currentQueue = new Queue();
                _currentQueue.AddStream(streamInfo);
                Queue.SetCurrentQueue(_currentQueue);
            }
            else Queue.GetCurrentQueue().AddStream(streamInfo);

            if (_audioClient == null) await JoinChannel();

            while (!Queue.GetCurrentQueue().IsEmpty())
            {
                Console.WriteLine(Queue.GetCurrentQueue().GetCount());
                var memoryStream = new MemoryStream();
                Stream stream = null;
                try
                {
                    stream = await youtube.Videos.Streams.GetAsync(Queue.GetCurrentQueue().GetNextStream());
                }catch(Exception e) { Console.WriteLine(e.ToString());  }

                bool songPlaying = Queue.GetCurrentQueue().GetCurrentStream() != null;

                // song info embed
                var embed = new EmbedBuilder();
                if (songPlaying) {
                    int queueCount = Queue.GetCurrentQueue().GetCount();
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
                    embed.AddField("channel", $"playing in {_channel.Name}!");
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

                using (var discord = _audioClient.CreatePCMStream(AudioApplication.Mixed))
                {
                    Queue.GetCurrentQueue().SetCurrentStream();
                    try { await discord.WriteAsync(memoryStream.ToArray(), 0, (int)memoryStream.Length); }
                    finally
                    {
                        await message.DeleteAsync();
                        Queue.GetCurrentQueue().Dequeue();
                        Queue.GetCurrentQueue().SetCurrentStreamNull();
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
