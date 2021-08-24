using AniListClient.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Globalization;

namespace AniListClient.Responses
{
    internal partial class CharactersFromAnimeResponse
    {
        [JsonProperty("Media", NullValueHandling = NullValueHandling.Ignore)]
        public MediaResponse Media { get; set; }

        internal partial class MediaResponse
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
            public Title Title { get; set; }

            [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
            public string Description { get; set; }

            [JsonProperty("coverImage", NullValueHandling = NullValueHandling.Ignore)]
            public Image CoverImage { get; set; }

            [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
            public Models.MediaStatus Status { get; set; }

            [JsonProperty("characters", NullValueHandling = NullValueHandling.Ignore)]
            public CharactersResponse Characters { get; set; }
        }

        internal partial class CharactersResponse : IHasPageInfo
        {
            [JsonProperty("edges", NullValueHandling = NullValueHandling.Ignore)]
            public CharacterEdge[] Edges { get; set; }

            [JsonProperty("pageInfo", NullValueHandling = NullValueHandling.Ignore)]
            public PageInfo PageInfo { get; set; }
        }

        internal partial class CharacterEdge
        {
            [JsonProperty("node", NullValueHandling = NullValueHandling.Ignore)]
            public CharacterNode Node { get; set; }

            [JsonProperty("role", NullValueHandling = NullValueHandling.Ignore)]
            public CharacterRole? Role { get; set; }

            [JsonProperty("voiceActors", NullValueHandling = NullValueHandling.Ignore)]
            public VoiceActorResponse[] VoiceActors { get; set; }
        }

        internal partial class CharacterNode
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
            public Name Name { get; set; }

            [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
            public Image Image { get; set; }
        }

        internal partial class VoiceActorResponse
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
            public Name Name { get; set; }
        }
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                RoleConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class RoleConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(CharacterRole) || t == typeof(CharacterRole?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "BACKGROUND":
                    return CharacterRole.Background;
                case "SUPPORTING":
                    return CharacterRole.Supporting;
                case "MAIN":
                    return CharacterRole.Main;
            }
            throw new Exception("Cannot unmarshal type Role");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (CharacterRole)untypedValue;
            switch (value)
            {
                case CharacterRole.Background:
                    serializer.Serialize(writer, "BACKGROUND");
                    return;
                case CharacterRole.Supporting:
                    serializer.Serialize(writer, "SUPPORTING");
                    return;
                case CharacterRole.Main:
                    serializer.Serialize(writer, "MAIN");
                    return;
            }
            throw new Exception("Cannot marshal type Role");
        }

        public static readonly RoleConverter Singleton = new RoleConverter();
    }
}
