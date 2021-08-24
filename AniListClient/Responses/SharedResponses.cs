using Newtonsoft.Json;
using System;

namespace AniListClient.Responses
{
    internal class Name
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

    internal class PageInfo
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

    internal class Image
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

    internal class Title
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
}
