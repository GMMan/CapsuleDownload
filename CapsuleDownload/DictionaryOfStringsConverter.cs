using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CapsuleDownload
{
    class DictionaryOfStringsConverter : JsonConverter<Dictionary<string, List<string>>>
    {
        public override Dictionary<string, List<string>> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Unexpected token type.");

            var dict = new Dictionary<string, List<string>>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string key = reader.GetString();
                    var value = new List<string>();
                    if (!reader.Read()) throw new JsonException("Cannot read from reader.");
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        value.Add(reader.GetString());
                    }
                    else if (reader.TokenType == JsonTokenType.StartArray)
                    {
                        while (reader.Read())
                        {
                            if (reader.TokenType == JsonTokenType.String)
                            {
                                value.Add(reader.GetString());
                            }
                            else if (reader.TokenType == JsonTokenType.EndArray)
                            {
                                break;
                            }
                            else
                            {
                                throw new JsonException("Unexpected token type.");
                            }
                        }
                    }
                    else if (reader.TokenType == JsonTokenType.Null)
                    {
                        value = null;
                    }
                    else
                    {
                        throw new JsonException("Unexpected token type.");
                    }
                    dict.Add(key, value);
                }
                else if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }
                else
                {
                    throw new JsonException("Unexpected token type.");
                }
            }
            return dict;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, List<string>> value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
