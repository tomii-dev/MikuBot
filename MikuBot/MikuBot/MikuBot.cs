using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MikuBot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MikuBot
{
    class MikuBot
    {
        public static void Main(string[] args)
        => new MikuBot().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandService _commands;

        public async Task MainAsync()
        {
            Config.Load();

            _client = new DiscordSocketClient();
            _client.Log += Log;

            _commands = new CommandService();

            var services = ConfigureServices(_client, _commands);

            await _client.LoginAsync(TokenType.Bot, Config.Token);
            await _client.StartAsync();

            _client.JoinedGuild += Database.AddGuild;

            await services.GetRequiredService<CommandHandler>().InitialiseAsync();

            await Extras.StatusLoop(_client);

            await Task.Delay(-1);
        }

        public ServiceProvider ConfigureServices(DiscordSocketClient client, CommandService commands)
        {
            return new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .AddSingleton<CommandHandler>()
                .BuildServiceProvider();
        }

        private Task Log(LogMessage msg)
        {
            return Task.CompletedTask;
        }
    }
}
