using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapsuleDownload.Models
{
    public class Box
    {
        [JsonPropertyName("keys")]
        public Dictionary<string, string> Keys { get; set; }
        [JsonPropertyName("missing_keys")]
        public List<string> MissingKeys { get; set; }
    }
}
