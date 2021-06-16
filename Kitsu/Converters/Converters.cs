using Kitsu.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Globalization;
using static Kitsu.Responses.UserLibraryGetRequest;

namespace Kitsu.Converters
{

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                DataTypeConverter.Singleton,
                ShowTypeEnumConverter.Singleton,
                LibraryEventKindConverter.Singleton,
                LibraryEventDataTypeConverter.Singleton,
                LibraryEventDatumTypeConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };

        internal class DataTypeConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(DataType) || t == typeof(DataType?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                switch (value)
                {
                    case "anime":
                        return DataType.Anime;
                    case "mappings":
                        return DataType.Mappings;
                }
                throw new Exception("Cannot unmarshal type DataType");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (DataType)untypedValue;
                switch (value)
                {
                    case DataType.Anime:
                        serializer.Serialize(writer, "anime");
                        return;
                    case DataType.Mappings:
                        serializer.Serialize(writer, "mappings");
                        return;
                }
                throw new Exception("Cannot marshal type DataType");
            }

            public static readonly DataTypeConverter Singleton = new DataTypeConverter();
        }

        internal class ShowTypeEnumConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(ShowTypeEnum) || t == typeof(ShowTypeEnum?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                switch (value.ToLower())
                {
                    case "tv":
                        return ShowTypeEnum.TV;
                    case "movie":
                        return ShowTypeEnum.Movie;
                    case "special":
                        return ShowTypeEnum.Special;
                    case "ova":
                        return ShowTypeEnum.OVA;
                }

                return ShowTypeEnum.OVA;
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (ShowTypeEnum)untypedValue;
                switch (value)
                {
                    case ShowTypeEnum.TV:
                        serializer.Serialize(writer, "TV");
                        return;
                    case ShowTypeEnum.Movie:
                        serializer.Serialize(writer, "movie");
                        return;
                }
                throw new Exception("Cannot marshal type ShowTypeEnum");
            }

            public static readonly ShowTypeEnumConverter Singleton = new ShowTypeEnumConverter();
        }

        internal class ParseStringConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                long l;
                if (long.TryParse(value, out l))
                {
                    return l;
                }
                throw new Exception("Cannot unmarshal type long");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (long)untypedValue;
                serializer.Serialize(writer, value.ToString());
                return;
            }

            public static readonly ParseStringConverter Singleton = new ParseStringConverter();
        }

        internal class LibraryEventKindConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(UserLibraryEventGetResponse.Kind) || t == typeof(UserLibraryEventGetResponse.Kind?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                switch (value)
                {
                    case "progressed":
                        return UserLibraryEventGetResponse.Kind.Progressed;
                    case "updated":
                        return UserLibraryEventGetResponse.Kind.Updated;
                }
                return null;
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (UserLibraryEventGetResponse.Kind)untypedValue;
                switch (value)
                {
                    case UserLibraryEventGetResponse.Kind.Progressed:
                        serializer.Serialize(writer, "progressed");
                        return;
                    case UserLibraryEventGetResponse.Kind.Updated:
                        serializer.Serialize(writer, "updated");
                        return;
                }
                throw new Exception("Cannot marshal type Kind");
            }

            public static readonly LibraryEventKindConverter Singleton = new LibraryEventKindConverter();
        }

        internal class LibraryEventDataTypeConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(UserLibraryEventGetResponse.DataType) || t == typeof(UserLibraryEventGetResponse.DataType?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                if (value == "libraryEntries")
                {
                    return UserLibraryEventGetResponse.DataType.LibraryEntries;
                }
                throw new Exception("Cannot unmarshal type DataType");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (UserLibraryEventGetResponse.DataType)untypedValue;
                if (value == UserLibraryEventGetResponse.DataType.LibraryEntries)
                {
                    serializer.Serialize(writer, "libraryEntries");
                    return;
                }
                throw new Exception("Cannot marshal type DataType");
            }

            public static readonly LibraryEventDataTypeConverter Singleton = new LibraryEventDataTypeConverter();
        }

        internal class LibraryEventDatumTypeConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(UserLibraryEventGetResponse.DatumType) || t == typeof(UserLibraryEventGetResponse.DatumType?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                if (value == "libraryEvents")
                {
                    return UserLibraryEventGetResponse.DatumType.LibraryEvents;
                }
                throw new Exception("Cannot unmarshal type DatumType");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (UserLibraryEventGetResponse.DatumType)untypedValue;
                if (value == UserLibraryEventGetResponse.DatumType.LibraryEvents)
                {
                    serializer.Serialize(writer, "libraryEvents");
                    return;
                }
                throw new Exception("Cannot marshal type DatumType");
            }

            public static readonly LibraryEventDatumTypeConverter Singleton = new LibraryEventDatumTypeConverter();
        }
    }
}
