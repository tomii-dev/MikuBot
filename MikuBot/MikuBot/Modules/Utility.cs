using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Discord;
using MikuBot.Services;

namespace MikuBot.Modules
{
    class Utility : ModuleBase
    {
        private static DiscordSocketClient _client;
        public Utility(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
        }

        [Command("ping", RunMode = RunMode.Async)]
        public async Task Ping()
        {
            var embed = new EmbedBuilder();
            embed.Title = "miku bot";
            embed.Description = "miku is online!";
            embed.ThumbnailUrl = _client.CurrentUser.GetAvatarUrl();
            embed.AddField("status", "ALIVE", true);
            embed.AddField("gateway server latency", $"{_client.Latency} ms");
            embed.AddField("data api latency", $"{Database.GetLatency().Result} ms");
            embed.WithColor(Color.Blue);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("help", RunMode=RunMode.Async)]
        public async Task Help()
        {

        }
    }
}
