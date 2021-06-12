﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using Kitsu.Responses;
//
//    var welcome = Welcome.FromJson(jsonString);

namespace Kitsu.Responses
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using static Kitsu.Responses.UserLibraryGetRequest;

    public partial class UserLibraryGetRequest
    {
        [JsonProperty("data")]
        public UserLibraryGet[] Data { get; set; }

        [JsonProperty("included")]
        public IncludedData[] Included { get; set; }

        [JsonProperty("meta")]
        public WelcomeMeta Meta { get; set; }

        [JsonProperty("links")]
        public WelcomeLinks Links { get; set; }

        public partial class UserLibraryGet
        {
            [JsonProperty("id")]
            [JsonConverter(typeof(Converter.ParseStringConverter))]
            public long? Id { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("links")]
            public DatumLinks Links { get; set; }

            [JsonProperty("attributes")]
            public DatumAttributes Attributes { get; set; }

            [JsonProperty("relationships")]
            public DatumRelationships Relationships { get; set; }
        }

        public partial class DatumAttributes
        {
            [JsonProperty("createdAt")]
            public DateTimeOffset? CreatedAt { get; set; }

            [JsonProperty("updatedAt")]
            public DateTimeOffset? UpdatedAt { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("progress")]
            public long? Progress { get; set; }

            [JsonProperty("volumesOwned")]
            public long? VolumesOwned { get; set; }

            [JsonProperty("reconsuming")]
            public bool Reconsuming { get; set; }

            [JsonProperty("reconsumeCount")]
            public long? ReconsumeCount { get; set; }

            [JsonProperty("notes")]
            public object Notes { get; set; }

            [JsonProperty("private")]
            public bool Private { get; set; }

            [JsonProperty("reactionSkipped")]
            public ReactionSkipped ReactionSkipped { get; set; }

            [JsonProperty("progressedAt")]
            public DateTimeOffset? ProgressedAt { get; set; }

            [JsonProperty("startedAt")]
            public DateTimeOffset? StartedAt { get; set; }

            [JsonProperty("finishedAt")]
            public DateTimeOffset? FinishedAt { get; set; }

            [JsonProperty("rating")]
            public string Rating { get; set; }

            [JsonProperty("ratingTwenty")]
            public long? RatingTwenty { get; set; }
        }

        public partial class DatumLinks
        {
            [JsonProperty("self")]
            public Uri Self { get; set; }
        }

        public partial class DatumRelationships
        {
            [JsonProperty("user")]
            public TartuGecko User { get; set; }

            [JsonProperty("anime")]
            public Anime Anime { get; set; }

            [JsonProperty("manga")]
            public TartuGecko Manga { get; set; }

            [JsonProperty("drama")]
            public TartuGecko Drama { get; set; }

            [JsonProperty("review")]
            public TartuGecko Review { get; set; }

            [JsonProperty("mediaReaction")]
            public TartuGecko MediaReaction { get; set; }

            [JsonProperty("media")]
            public TartuGecko Media { get; set; }

            [JsonProperty("unit")]
            public TartuGecko Unit { get; set; }

            [JsonProperty("nextUnit")]
            public TartuGecko NextUnit { get; set; }
        }

        public partial class Anime
        {
            [JsonProperty("links")]
            public PurpleLinks Links { get; set; }

            [JsonProperty("data")]
            public Dat Data { get; set; }
        }

        public partial class Dat
        {
            [JsonProperty("type")]
            public DataType Type { get; set; }

            [JsonProperty("id")]
            [JsonConverter(typeof(Converter.ParseStringConverter))]
            public long? Id { get; set; }
        }

        public partial class PurpleLinks
        {
            [JsonProperty("self")]
            public Uri Self { get; set; }

            [JsonProperty("related")]
            public Uri Related { get; set; }
        }

        public partial class TartuGecko
        {
            [JsonProperty("links")]
            public PurpleLinks Links { get; set; }
        }

        public partial class IncludedData
        {
            [JsonProperty("id")]
            [JsonConverter(typeof(Converter.ParseStringConverter))]
            public long Id { get; set; }

            [JsonProperty("type")]
            public DataType Type { get; set; }

            [JsonProperty("links")]
            public DatumLinks Links { get; set; }

            [JsonProperty("attributes")]
            public IncludedAttributes Attributes { get; set; }

            [JsonProperty("relationships")]
            public IncludedRelationships Relationships { get; set; }
        }

        public partial class IncludedAttributes
        {
            [JsonProperty("createdAt")]
            public DateTimeOffset? CreatedAt { get; set; }

            [JsonProperty("updatedAt")]
            public DateTimeOffset? UpdatedAt { get; set; }

            [JsonProperty("slug", NullValueHandling = NullValueHandling.Ignore)]
            public string Slug { get; set; }

            [JsonProperty("synopsis", NullValueHandling = NullValueHandling.Ignore)]
            public string Synopsis { get; set; }

            [JsonProperty("coverImageTopOffset", NullValueHandling = NullValueHandling.Ignore)]
            public long? CoverImageTopOffset { get; set; }

            [JsonProperty("titles", NullValueHandling = NullValueHandling.Ignore)]
            public Titles Titles { get; set; }

            [JsonProperty("canonicalTitle", NullValueHandling = NullValueHandling.Ignore)]
            public string CanonicalTitle { get; set; }

            [JsonProperty("abbreviatedTitles", NullValueHandling = NullValueHandling.Ignore)]
            public string[] AbbreviatedTitles { get; set; }

            [JsonProperty("averageRating", NullValueHandling = NullValueHandling.Ignore)]
            public string AverageRating { get; set; }

            [JsonProperty("ratingFrequencies", NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, long> RatingFrequencies { get; set; }

            [JsonProperty("userCount", NullValueHandling = NullValueHandling.Ignore)]
            public long? UserCount { get; set; }

            [JsonProperty("favoritesCount", NullValueHandling = NullValueHandling.Ignore)]
            public long? FavoritesCount { get; set; }

            [JsonProperty("startDate", NullValueHandling = NullValueHandling.Ignore)]
            public DateTimeOffset? StartDate { get; set; }

            [JsonProperty("endDate", NullValueHandling = NullValueHandling.Ignore)]
            public DateTimeOffset? EndDate { get; set; }

            [JsonProperty("nextRelease")]
            public object NextRelease { get; set; }

            [JsonProperty("popularityRank", NullValueHandling = NullValueHandling.Ignore)]
            public long? PopularityRank { get; set; }

            [JsonProperty("ratingRank", NullValueHandling = NullValueHandling.Ignore)]
            public long? RatingRank { get; set; }

            [JsonProperty("ageRating", NullValueHandling = NullValueHandling.Ignore)]
            public string AgeRating { get; set; }

            [JsonProperty("ageRatingGuide", NullValueHandling = NullValueHandling.Ignore)]
            public string AgeRatingGuide { get; set; }

            [JsonProperty("subtype", NullValueHandling = NullValueHandling.Ignore)]
            public ShowTypeEnum? Subtype { get; set; }

            [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
            public string Status { get; set; }

            [JsonProperty("tba")]
            public string Tba { get; set; }

            [JsonProperty("posterImage", NullValueHandling = NullValueHandling.Ignore)]
            public PosterImage PosterImage { get; set; }

            [JsonProperty("coverImage", NullValueHandling = NullValueHandling.Ignore)]
            public CoverImage CoverImage { get; set; }

            [JsonProperty("episodeCount", NullValueHandling = NullValueHandling.Ignore)]
            public long? EpisodeCount { get; set; }

            [JsonProperty("episodeLength", NullValueHandling = NullValueHandling.Ignore)]
            public long? EpisodeLength { get; set; }

            [JsonProperty("totalLength", NullValueHandling = NullValueHandling.Ignore)]
            public long? TotalLength { get; set; }

            [JsonProperty("youtubeVideoId", NullValueHandling = NullValueHandling.Ignore)]
            public string YoutubeVideoId { get; set; }

            [JsonProperty("showType", NullValueHandling = NullValueHandling.Ignore)]
            public ShowTypeEnum? ShowType { get; set; }

            [JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
            public bool? Nsfw { get; set; }

            [JsonProperty("externalSite", NullValueHandling = NullValueHandling.Ignore)]
            public string ExternalSite { get; set; }

            [JsonProperty("externalId", NullValueHandling = NullValueHandling.Ignore)]
            public string ExternalId { get; set; }
        }

        public partial class CoverImage
        {
            [JsonProperty("tiny")]
            public Uri Tiny { get; set; }

            [JsonProperty("small")]
            public Uri Small { get; set; }

            [JsonProperty("large")]
            public Uri Large { get; set; }

            [JsonProperty("original")]
            public Uri Original { get; set; }

            [JsonProperty("meta")]
            public CoverImageMeta Meta { get; set; }
        }

        public partial class CoverImageMeta
        {
            [JsonProperty("dimensions")]
            public PurpleDimensions Dimensions { get; set; }
        }

        public partial class PurpleDimensions
        {
            [JsonProperty("tiny")]
            public Large Tiny { get; set; }

            [JsonProperty("small")]
            public Large Small { get; set; }

            [JsonProperty("large")]
            public Large Large { get; set; }
        }

        public partial class Large
        {
            [JsonProperty("width")]
            public long? Width { get; set; }

            [JsonProperty("height")]
            public long? Height { get; set; }
        }

        public partial class PosterImage
        {
            [JsonProperty("tiny")]
            public Uri Tiny { get; set; }

            [JsonProperty("small")]
            public Uri Small { get; set; }

            [JsonProperty("medium")]
            public Uri Medium { get; set; }

            [JsonProperty("large")]
            public Uri Large { get; set; }

            [JsonProperty("original")]
            public Uri Original { get; set; }

            [JsonProperty("meta")]
            public PosterImageMeta Meta { get; set; }
        }

        public partial class PosterImageMeta
        {
            [JsonProperty("dimensions")]
            public FluffyDimensions Dimensions { get; set; }
        }

        public partial class FluffyDimensions
        {
            [JsonProperty("tiny")]
            public Large Tiny { get; set; }

            [JsonProperty("small")]
            public Large Small { get; set; }

            [JsonProperty("medium")]
            public Large Medium { get; set; }

            [JsonProperty("large")]
            public Large Large { get; set; }
        }

        public partial class Titles
        {
            [JsonProperty("en")]
            public string En { get; set; }

            [JsonProperty("en_jp")]
            public string EnJp { get; set; }

            [JsonProperty("ja_jp")]
            public string JaJp { get; set; }

            [JsonProperty("en_us", NullValueHandling = NullValueHandling.Ignore)]
            public string EnUs { get; set; }
        }

        public partial class IncludedRelationships
        {
            [JsonProperty("genres", NullValueHandling = NullValueHandling.Ignore)]
            public TartuGecko Genres { get; set; }

            [JsonProperty("categories", NullValueHandling = NullValueHandling.Ignore)]
            public TartuGecko Categories { get; set; }

            [JsonProperty("castings", NullValueHandling = NullValueHandling.Ignore)]
            public TartuGecko Castings { get; set; }

            [JsonProperty("installments", NullValueHandling = NullValueHandling.Ignore)]
            public TartuGecko Installments { get; set; }

            [JsonProperty("mappings", NullValueHandling = NullValueHandling.Ignore)]
            public Mappings Mappings { get; set; }

            [JsonProperty("reviews", NullValueHandling = NullValueHandling.Ignore)]
            public TartuGecko Reviews { get; set; }

            [JsonProperty("mediaRelationships", NullValueHandling = NullValueHandling.Ignore)]
            public TartuGecko MediaRelationships { get; set; }

            [JsonProperty("characters", NullValueHandling = NullValueHandling.Ignore)]
            public TartuGecko Characters { get; set; }

            [JsonProperty("staff", NullValueHandling = NullValueHandling.Ignore)]
            public TartuGecko Staff { get; set; }

            [JsonProperty("productions", NullValueHandling = NullValueHandling.Ignore)]
            public TartuGecko Productions { get; set; }

            [JsonProperty("quotes", NullValueHandling = NullValueHandling.Ignore)]
            public TartuGecko Quotes { get; set; }

            [JsonProperty("episodes", NullValueHandling = NullValueHandling.Ignore)]
            public TartuGecko Episodes { get; set; }

            [JsonProperty("streamingLinks", NullValueHandling = NullValueHandling.Ignore)]
            public TartuGecko StreamingLinks { get; set; }

            [JsonProperty("animeProductions", NullValueHandling = NullValueHandling.Ignore)]
            public TartuGecko AnimeProductions { get; set; }

            [JsonProperty("animeCharacters", NullValueHandling = NullValueHandling.Ignore)]
            public TartuGecko AnimeCharacters { get; set; }

            [JsonProperty("animeStaff", NullValueHandling = NullValueHandling.Ignore)]
            public TartuGecko AnimeStaff { get; set; }

            [JsonProperty("item", NullValueHandling = NullValueHandling.Ignore)]
            public TartuGecko Item { get; set; }
        }

        public partial class Mappings
        {
            [JsonProperty("links")]
            public PurpleLinks Links { get; set; }

            [JsonProperty("data")]
            public Dat[] Data { get; set; }
        }

        public partial class WelcomeLinks
        {
            [JsonProperty("first")]
            public Uri First { get; set; }

            [JsonProperty("next")]
            public Uri Next { get; set; }

            [JsonProperty("last")]
            public Uri Last { get; set; }
        }

        public partial class WelcomeMeta
        {
            [JsonProperty("statusCounts")]
            public StatusCounts StatusCounts { get; set; }

            [JsonProperty("count")]
            public long? Count { get; set; }
        }

        public partial class StatusCounts
        {
            [JsonProperty("current")]
            public long? Current { get; set; }

            [JsonProperty("completed")]
            public long? Completed { get; set; }

            [JsonProperty("dropped")]
            public long? Dropped { get; set; }

            [JsonProperty("onHold")]
            public long? OnHold { get; set; }

            [JsonProperty("planned")]
            public long? Planned { get; set; }
        }

        public enum ReactionSkipped { Unskipped };

        public enum DataType { Anime, Mappings };

        public enum ShowTypeEnum { Movie, TV, OVA, Special };

        public static UserLibraryGetRequest FromJson(string json) => JsonConvert.DeserializeObject<UserLibraryGetRequest>(json, Kitsu.Responses.Converter.Settings);

    }

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
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };

        internal class ParseStringConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                long l;
                if (Int64.TryParse(value, out l))
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
    }
}
