using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MikuBot.Services
{
    class TransactionInfo
    {
        [JsonPropertyName("senderId")]
        public string SenderId { get; set; }

        [JsonPropertyName("recipientId")]
        public string RecipientId { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }
    }
}
