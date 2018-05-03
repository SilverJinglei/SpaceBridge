using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SpaceStation.Core
{
#if DEBUG
    public
#else
    internal 
#endif
        class Metadata
    {
        [JsonConverter(typeof (StringEnumConverter))]
        public MetadataType Type { get; set; }

        public string Token { get; set; }

        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public virtual bool IsValid()
        {
            return false;
        }

        public static Metadata Parse(string deserializedString)
        {
            return JsonConvert.DeserializeObject<Metadata>(deserializedString);
        }
    }

#if DEBUG
    public
#else
    internal 
#endif
    class OperationMetadata : Metadata
    {
        public string Class { get; set; }

        public string Name { get; set; }

        public IDictionary<string, object> Parameters { get; set; }

        public bool IsOneWay { get; set; }

        public OperationMetadata()
        {
            Type = MetadataType.Operation;
        }

        public override bool IsValid()
        {
            if (Type != MetadataType.Operation)
                return false;

            if (string.IsNullOrEmpty(Class))
                return false;

            return string.IsNullOrEmpty(Name);
        }

        public new static OperationMetadata Parse(string deserializedString)
        {
            return JsonConvert.DeserializeObject<OperationMetadata>(deserializedString);
        }
    }

#if DEBUG
    public
#else
    internal 
#endif
    class ResultMetadata : Metadata
    {
        //[JsonConverter(typeof(Int32Converter))]
        public object Value { get; set; }

        public bool IsException { get; set; }

        public ResultMetadata()
        {
            Type = MetadataType.Result;
        }

        public new static ResultMetadata Parse(string deserializedString)
        {
            return JsonConvert.DeserializeObject<ResultMetadata>(deserializedString);
        }

        private static ResultMetadata _empty;

        public static ResultMetadata Empty => _empty ?? (_empty = new ResultMetadata());
    }

#if DEBUG
    public
#else
    internal 
#endif
    enum MetadataType
    {
        Operation,
        Result
    }

    public class Int32Converter : JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType) => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                    return (int)(serializer.Deserialize<long>(reader));
                case JsonToken.None:
                    break;
                case JsonToken.StartObject:
                    break;
                case JsonToken.StartArray:
                    break;
                case JsonToken.StartConstructor:
                    break;
                case JsonToken.PropertyName:
                    break;
                case JsonToken.Comment:
                    break;
                case JsonToken.Raw:
                    break;
                case JsonToken.Float:
                    break;
                case JsonToken.String:
                    break;
                case JsonToken.Boolean:
                    break;
                case JsonToken.Null:
                    break;
                case JsonToken.Undefined:
                    break;
                case JsonToken.EndObject:
                    break;
                case JsonToken.EndArray:
                    break;
                case JsonToken.EndConstructor:
                    break;
                case JsonToken.Date:
                    break;
                case JsonToken.Bytes:
                    break;
                default:
                    break;
            }

            return serializer.Deserialize(reader, objectType);
        }
    }
}