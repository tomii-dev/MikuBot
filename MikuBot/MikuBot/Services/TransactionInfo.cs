using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MikuBot.Services
{
    class TransactionInfo
    {
        public string senderId { get; set; }

        public string recipientId { get; set; }

        public int amount { get; set; }
    }
}
