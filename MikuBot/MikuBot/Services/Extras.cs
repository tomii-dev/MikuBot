using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Discord;

namespace MikuBot.Services
{
    class Extras
    {
        public static async Task LoadingMessage(string content, ICommandContext context, int count)
        {
            var message = await context.Channel.SendMessageAsync($"*{content}*");
            while(count > 0)
            {
                await message.ModifyAsync(msg => msg.Content = $"*{content}*");
                await message.ModifyAsync(msg => msg.Content = $"*{content}.*");
                await Task.Delay(200);
                await message.ModifyAsync(msg => msg.Content = $"*{content}..*");
                await Task.Delay(200);
                await message.ModifyAsync(msg => msg.Content = $"*{content}...*");
                await Task.Delay(200);
                count--;
            }
            await message.DeleteAsync();
        }

        public static async Task StatusLoop(DiscordSocketClient client)
        {
            while (true)
            {
                var check = await Database.CheckApi();
                Console.WriteLine(check);

                if (!check)
                {
                    Console.WriteLine("man wtf");
                    await client.SetStatusAsync(UserStatus.AFK);
                    await client.SetGameAsync("data api offline :/");
                }
                else
                {
                    await client.SetStatusAsync(UserStatus.Online);
                    await client.SetGameAsync("project diva");
                }

                await Task.Delay(10000);
            }
        }
    }
}
