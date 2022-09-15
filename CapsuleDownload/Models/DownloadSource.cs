using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapsuleDownload.Models
{
    public class DownloadSource
    {
        [JsonPropertyName("location")]
        public string Location { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
