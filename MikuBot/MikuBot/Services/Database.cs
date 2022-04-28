using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MikuBot.Services
{
    public static class Database
    {
        private static readonly HttpClient _client = new HttpClient();
        private static DiscordSocketClient _dClient;
        private static string _apiUrl = "http://127.0.0.1:5000/mikuapi";
        public static async Task<bool> CheckApi()
        {
            var check = _client.GetAsync(_apiUrl);
            if (await Task.WhenAny(check, Task.Delay(100)) == check) return true;
            else return false;
        }
        public static async Task<string> GetGuildPrefix(string id)
        {
            var response = await _client.GetAsync($"{_apiUrl}/getprefix/{id}");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();
            else return "!";
        }
        public static async Task ChangeGuildPrefix(string id, string prefix)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("prefix", prefix);
            var json = JsonConvert.SerializeObject(dict, Formatting.Indented);
            var content = new StringContent(
                json, Encoding.UTF8, "application/json");
            await _client.PostAsync($"{_apiUrl}/changeprefix/{id}", content);
        }

        public static async Task<int> GetLatency()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            var result = await _client.GetAsync(_apiUrl);
            timer.Stop();
            return (int)timer.ElapsedMilliseconds;
        }

        public static async Task AddGuild(IGuild guild)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("id", guild.Id.ToString());
            dict.Add("name", guild.Name);
            var json = JsonConvert.SerializeObject(dict, Formatting.Indented);
            var content = new StringContent(
                json, Encoding.UTF8, "application/json");
            await _client.PostAsync($"{_apiUrl}/addguild", content);
        }

        public static async Task RemoveGuild(IGuild guild)
        {

        }

        public static async Task CreateAccount(IGuildUser user)
        {
            StringContent content = new StringContent("create", Encoding.UTF8, "application/txt");
            await _client.PostAsync($"{_apiUrl}/bank/createaccount/{user.Id}", content);
        }

        public static async Task<string> GetBalance(IGuildUser user)
        {
            var response = await _client.GetAsync($"{_apiUrl}/bank/getbalance/{user.Id.ToString()}");
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task AddFunds(IGuildUser user, int amount)
        {
            TransactionInfo info = new TransactionInfo() { 
                recipientId = user.Id.ToString(), amount = amount };
            var json = JsonConvert.SerializeObject(info, Formatting.Indented);
            var content = new StringContent(
                json, Encoding.UTF8, "application/json");
            await _client.PostAsync($"{_apiUrl}/bank/addfunds", content);
        }

        public static async Task SubtractFunds(IGuildUser user, int amount)
        {
            TransactionInfo info = new TransactionInfo(){
                recipientId = user.Id.ToString(), amount = amount*-1};
            var json = JsonConvert.SerializeObject(info, Formatting.Indented);
            var content = new StringContent(
                json, Encoding.UTF8, "application/json");
            await _client.PostAsync($"{_apiUrl}/bank/subtractfunds", content);
        }

        public static async Task Transfer(IGuildUser sender, string recipientId, int amount)
        {
            TransactionInfo info = new TransactionInfo()
            {
                senderId = sender.Id.ToString(),
                recipientId = recipientId,
                amount = amount
            };
            var json = JsonConvert.SerializeObject(info);
            var content = new StringContent(
                json, Encoding.UTF8, "application/json");
            await _client.PostAsync($"{_apiUrl}/bank/transfer", content);
        }
    }
}
