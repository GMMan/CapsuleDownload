using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapsuleDownload.Models
{
    public class User
    {
        [JsonPropertyName("token_url")]
        public string TokenUrl { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("user_name")]
        public string Name { get; set; }
        [JsonPropertyName("user_currency")]
        public string Currency { get; set; }
    }
}
