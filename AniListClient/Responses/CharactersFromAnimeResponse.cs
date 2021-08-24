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
}
