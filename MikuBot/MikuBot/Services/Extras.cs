using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

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
    }
}
