using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MikuBot.Modules;
using MikuBot.Modules.Voice;

namespace MikuBot.Services
{
    public class CommandHandler
    {
        private CommandService _commands;
        private DiscordSocketClient _client;
        private IServiceProvider _services;

        public CommandHandler(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
            _client.MessageReceived += HandleCommand;
        }
        
        private Task HandleCommand(SocketMessage rawMsg)
        {
            _ = Task.Run(async () =>
            {
                var msg = rawMsg as SocketUserMessage;
                if (msg == null) return;

                if (rawMsg.Author.IsBot) return;

                if (!Database.CheckApi().Result)
                {
                    await _client.SetStatusAsync(Discord.UserStatus.AFK);
                    await _client.SetGameAsync("data api offline :/");
                    return;
                }

                var chnl = msg.Channel as SocketGuildChannel;

                char prefix = Database.GetGuildPrefix(chnl.Guild.Id.ToString()).Result[0];

                if (msg.ToString()[0] != prefix) return;

                var context = new SocketCommandContext(_client, msg);

                var result = await _commands.ExecuteAsync(context, 1, _services);
                if (!result.IsSuccess)
                {
                    await msg.Channel.SendMessageAsync(result.ErrorReason);
                }
            });
            return Task.CompletedTask;
        }

        public async Task InitialiseAsync()
        {
            await _commands.AddModuleAsync<Admin>(_services);
            await _commands.AddModuleAsync<Utility>(_services);
            await _commands.AddModuleAsync<Voice>(_services);
            await _commands.AddModuleAsync<Economy>(_services);
        }
    }
}
