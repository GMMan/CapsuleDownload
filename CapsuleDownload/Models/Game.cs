using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapsuleDownload.Models
{
    // Note: only necessary data is deserialized, and models mixed and matched for better usability
    public class Game
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonIgnore]
        public Download Download { get; set; }
        [JsonIgnore]
        public Box Box { get; set; }
        [JsonIgnore]
        public GameInfo Info { get; set; }
    }
}
