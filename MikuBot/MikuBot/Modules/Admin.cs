using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using MikuBot.Services;
using Discord;

namespace MikuBot.Modules
{
    class Admin : ModuleBase
    {
        private static DiscordSocketClient _client;

        public Admin(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
        }

        [Command("change-prefix", RunMode=RunMode.Async)]
        public async Task ChangePrefix(string prefix)
        {
            await Database.ChangeGuildPrefix(Context.Guild.Id.ToString(), prefix);
            await Context.Channel.SendMessageAsync($"guild prefix changed to '{prefix}'!");
        }

        [Command("leave-server", RunMode=RunMode.Async)]
        public async Task LeaveServer()
        {
            await Context.Channel.SendMessageAsync("goodbye @everyone :0");
            await Context.Guild.LeaveAsync();
        }

        [Command("kick", RunMode=RunMode.Async)]
        public async Task Kick(IGuildUser user, string reason = null)
        {
            await user.KickAsync();
            await Context.Channel.SendMessageAsync($"{user.DisplayName} has been kicked\nreason: {reason}");
        }
    }
}
