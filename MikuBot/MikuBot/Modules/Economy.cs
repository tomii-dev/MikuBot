using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using System.Threading.Tasks;
using MikuBot.Services;

namespace MikuBot.Modules
{
    class Economy : ModuleBase
    {
        private static DiscordSocketClient _client;
        public Economy(IServiceProvider services)
        {
            _client = services.GetRequiredService<DiscordSocketClient>();
        }

        [Command("balance", RunMode=RunMode.Async)]
        public async Task GetBalance()
        {
            var balance = Database.GetBalance(Context.User as IGuildUser).Result;
            string prefix = Database.GetGuildPrefix(Context.Guild.Id.ToString()).Result;
            if (balance == "no account")
            {
                await ReplyAsync(
                    $"{Context.User.Mention} you do not have an account! " +
                    $"please use **{prefix}createaccount** to set one up ^-^");
                return;
            }
            var embed = new EmbedBuilder();
            embed.Title = $"{Context.User.Username}'s account";
            embed.ThumbnailUrl = Context.User.GetAvatarUrl();
            embed.AddField("balance", $"$^-^{balance}");
            embed.WithColor(Color.Blue);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("createaccount", RunMode=RunMode.Async)]
        public async Task CreateAccount()
        {
            string prefix = Database.GetGuildPrefix(Context.Guild.Id.ToString()).Result;

            if (Database.GetBalance(Context.User as IGuildUser).Result != "no account")
            {
                await ReplyAsync($"{Context.User.Mention} you **already** have an account! use {prefix}balance to check your account :p");
                return;
            }
            await Extras.LoadingMessage("creating account", Context, 1);
            await Database.CreateAccount(Context.User as IGuildUser);
            await ReplyAsync("**every new member of *MikuBank* gets a starting fund of 50 MikuCoin!**");
            await Extras.LoadingMessage("adding funds", Context, 1);
            await ReplyAsync(
                $"account created! your account number is **{Context.User.Id}**. " +
                $"others will need this to make transfers to your account!");
            await ReplyAsync($"you can view your account number at any point " +
                $"using {Database.GetGuildPrefix(Context.Guild.Id.ToString()).Result}bakance");
        }

        [Command("transfer", RunMode=RunMode.Async)]
        public async Task Transfer(int amount, string targetId)
        {
            if(Int32.Parse(Database.GetBalance(Context.User as IGuildUser).Result) - amount < 0)
            {
                await ReplyAsync("insufficient funds :/");
                return;
            }
            await Database.Transfer(Context.User as IGuildUser, targetId, amount);
            await ReplyAsync($"**${amount}** transferred to {_client.GetUserAsync(Convert.ToUInt64(targetId)).Result.Username}'s account!");
        }
    }
}
