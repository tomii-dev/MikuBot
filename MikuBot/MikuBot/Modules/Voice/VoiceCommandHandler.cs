using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace MikuBot.Modules.Voice
{
    class VoiceCommandHandler
    {
        private static DiscordSocketClient _client;
        private static ICommandContext _context;
        private static SocketUser _user;
        public VoiceCommandHandler(DiscordSocketClient client, string command, ICommandContext context, SocketUser user)
        {
            _client = client;
            _context = context;
            _user = user;
            HandleCommand(command);
        }

        private void HandleCommand(string command)
        {
            switch (command)
            {
                case "hi miku":
                    {
                        _context.Channel.SendMessageAsync($"hi {_user.Username}");
                        break;
                    }
                case "miku fuck off":
                    {
                        Voice.GetChannel().DisconnectAsync();
                        break;
                    }
            }
        }
    }
}
