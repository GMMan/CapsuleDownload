using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CapsuleDownload.Models
{
    public class Crypto
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("key")]
        public string Key { get; set; }
        [JsonPropertyName("iv")]
        public string Iv { get; set; }

        [JsonIgnore]
        public byte[] KeyBytes
        {
            get
            {
                return HexToBytes(Key);
            }
            set
            {
                Key = BytesToHex(value);
            }
        }

        [JsonIgnore]
        public byte[] IvBytes
        {
            get
            {
                return HexToBytes(Iv);
            }
            set
            {
                Iv = BytesToHex(value);
            }
        }

        static byte[] HexToBytes(string s)
        {
            if (s == null) return null;
            if (s.Length % 2 != 0) throw new ArgumentException("String does not have even length.", nameof(s));
            byte[] b = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
            {
                b[i / 2] = byte.Parse(s.Substring(i, 2), NumberStyles.HexNumber);
            }
            return b;
        }

        static string BytesToHex(byte[] b)
        {
            if (b == null) return null;
            StringBuilder sb = new StringBuilder();
            foreach (var bt in b)
            {
                sb.Append($"{bt:X2}");
            }
            return sb.ToString();
        }
    }
}
