using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CapsuleDownload.Models
{
    public class Download
    {
        [JsonPropertyName("crypto")]
        public Dictionary<string, Crypto> Crypto { get; set; }
        [JsonPropertyName("downloads")]
        public List<DownloadSource> Sources { get; set; }
        [JsonConverter(typeof(DictionaryOfStringsConverter))]
        [JsonPropertyName("file_manifest")]
        public Dictionary<string, List<string>> FileManifest { get; set; }

        [JsonIgnore]
        public bool IsUsingParts => FileManifest.ContainsKey("${BIG_FILE}/Parts");

        public List<string> GenerateDownloadUrls()
        {
            List<string> files = new List<string>();

            // Modern Capsule apparently only downloads Big Files, even if loose files are present
            // Other files not guaranteed to be present in bucket
            string baseUrl = Sources[0].Location + "/big_file";
            if (IsUsingParts)
            {
                int partsCount = GetPartsCount();
                for (int i = 0; i < partsCount; ++i)
                {
                    files.Add($"{baseUrl}/{GetPartName(i)}");
                }
            }
            else
            {
                if (!FileManifest.ContainsKey("${BIG_FILE}/Full")) throw new Exception("Missing manifest entry for full Big File");
                files.Add($"{baseUrl}/fullgame");
            }

            return files;
        }

        public int GetPartsCount()
        {
            if (IsUsingParts)
            {
                return FileManifest["${BIG_FILE}/Parts"].Count;
            }
            else
            {
                throw new InvalidOperationException("Not using parts.");
            }
        }

        public static string GetPartName(int i)
        {
            return $"part{i + 1:d8}";
        }
    }
}
