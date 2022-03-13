using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MikuBot
{
    public static class Config
    {
        public const string ConfigPath = "miku.json";
        private static BotConfig _configuration;

        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            IgnoreNullValues = false,
            AllowTrailingCommas = true
        };

        public static void Load()
        {
            _configuration = JsonSerializer.Deserialize<BotConfig>(File.ReadAllText(ConfigPath), JsonOptions);
        }

        public static void ChangeConfig(string key, string value)
        {
            string json = File.ReadAllText(ConfigPath);
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            jsonObj[key] = value;
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(json);
            File.WriteAllText(ConfigPath, output);
        }

        public static string Token => _configuration.Token;
        public static string Status => _configuration.Status;

        private struct BotConfig
        {
            [JsonPropertyName("token")]
            public string Token { get; set;  }

            [JsonPropertyName("status")]
            public string Status { get; set; }
        }
    }
}
