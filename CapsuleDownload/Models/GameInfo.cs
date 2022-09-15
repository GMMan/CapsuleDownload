using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapsuleDownload.Models
{
    public class GameInfo
    {
        [JsonPropertyName("age_rating")]
        public string AgeRating { get; set; }
        [JsonPropertyName("banner")]
        public string Banner { get; set; }
        [JsonPropertyName("box_art")]
        public string BoxArt { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("review_url")]
        public string ReviewUrl { get; set; }
        [JsonPropertyName("size")]
        public string Size { get; set; }
    }
}
