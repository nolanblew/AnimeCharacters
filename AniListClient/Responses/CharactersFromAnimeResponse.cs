using System;
using System.Collections.Generic;

using System.Globalization;
using AniListClient.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AniListClient.Responses
{
    internal partial class CharactersFromAnimeResponse
    {
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public Data Data { get; set; }
    }

    internal partial class Data
    {
        [JsonProperty("Media", NullValueHandling = NullValueHandling.Ignore)]
        public Media Media { get; set; }
    }

    internal partial class Media
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
        public Characters Characters { get; set; }
    }

    internal partial class Characters : IHasPageInfo
    {
        [JsonProperty("edges", NullValueHandling = NullValueHandling.Ignore)]
        public CharacterEdge[] Edges { get; set; }

        [JsonProperty("pageInfo", NullValueHandling = NullValueHandling.Ignore)]
        public PageInfo PageInfo { get; set; }
    }

    internal partial class CharacterEdge
    {
        [JsonProperty("node", NullValueHandling = NullValueHandling.Ignore)]
        public Node Node { get; set; }

        [JsonProperty("role", NullValueHandling = NullValueHandling.Ignore)]
        public CharacterRole? Role { get; set; }

        [JsonProperty("voiceActors", NullValueHandling = NullValueHandling.Ignore)]
        public VoiceActor[] VoiceActors { get; set; }
    }

    internal partial class Node
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public Name Name { get; set; }

        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public Image Image { get; set; }
    }

    internal partial class VoiceActor
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public Name Name { get; set; }
    }

    internal partial class Name
    {
        [JsonProperty("romaji", NullValueHandling = NullValueHandling.Ignore)]
        public string Romaji { get; set; }

        [JsonProperty("first", NullValueHandling = NullValueHandling.Ignore)]
        public string First { get; set; }

        [JsonProperty("last", NullValueHandling = NullValueHandling.Ignore)]
        public string Last { get; set; }

        [JsonProperty("full", NullValueHandling = NullValueHandling.Ignore)]
        public string Full { get; set; }

        [JsonProperty("native", NullValueHandling = NullValueHandling.Ignore)]
        public string Native { get; set; }

        [JsonProperty("alternative", NullValueHandling = NullValueHandling.Ignore)]
        public string Alternative { get; set; }

        [JsonProperty("userPreferred", NullValueHandling = NullValueHandling.Ignore)]
        public string UserPreferred { get; set; }
    }

    internal partial class PageInfo
    {
        [JsonProperty("total", NullValueHandling = NullValueHandling.Ignore)]
        public long? Total { get; set; }

        [JsonProperty("perPage", NullValueHandling = NullValueHandling.Ignore)]
        public long? PerPage { get; set; }

        [JsonProperty("currentPage", NullValueHandling = NullValueHandling.Ignore)]
        public long? CurrentPage { get; set; }

        [JsonProperty("lastPage", NullValueHandling = NullValueHandling.Ignore)]
        public long? LastPage { get; set; }

        [JsonProperty("hasNextPage", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasNextPage { get; set; }
    }

    internal partial class Image
    {
        [JsonProperty("extraLarge", NullValueHandling = NullValueHandling.Ignore)]
        public Uri ExtraLarge { get; set; }

        [JsonProperty("large", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Large { get; set; }

        [JsonProperty("medium", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Medium { get; set; }

        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        public string Color { get; set; }
    }

    internal partial class Title
    {
        [JsonProperty("romaji", NullValueHandling = NullValueHandling.Ignore)]
        public string Romaji { get; set; }

        [JsonProperty("english", NullValueHandling = NullValueHandling.Ignore)]
        public string English { get; set; }

        [JsonProperty("native", NullValueHandling = NullValueHandling.Ignore)]
        public string Native { get; set; }

        [JsonProperty("userPreferred", NullValueHandling = NullValueHandling.Ignore)]
        public string UserPreferred { get; set; }
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
