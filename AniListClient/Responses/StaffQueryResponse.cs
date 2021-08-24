using Newtonsoft.Json;

namespace AniListClient.Responses
{
    namespace AniListClient.Responses
    {
        internal class StaffQueryResponse
        {
            [JsonProperty("Staff", NullValueHandling = NullValueHandling.Ignore)]
            public StaffResponse Staff { get; set; }

            internal class StaffResponse
            {
                [JsonProperty("id")]
                public int Id { get; set; }

                [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
                public Name Name { get; set; }

                [JsonProperty("languageV2", NullValueHandling = NullValueHandling.Ignore)]
                public Models.Language LanguageV2 { get; set; }

                [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
                public Image Image { get; set; }

                [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
                public string Description { get; set; }

                [JsonProperty("characters", NullValueHandling = NullValueHandling.Ignore)]
                public CharactersResponse Characters { get; set; }
            }

            internal class CharactersResponse : IHasPageInfo
            {
                [JsonProperty("edges", NullValueHandling = NullValueHandling.Ignore)]
                public CharacterEdgeResponse[] Edges { get; set; }

                [JsonProperty("pageInfo", NullValueHandling = NullValueHandling.Ignore)]
                public PageInfo PageInfo { get; set; }
            }

            internal class CharacterEdgeResponse
            {
                [JsonProperty("id")]
                public int Id { get; set; }

                [JsonProperty("role", NullValueHandling = NullValueHandling.Ignore)]
                public Models.CharacterRole? Role { get; set; }

                [JsonProperty("node", NullValueHandling = NullValueHandling.Ignore)]
                public NodeResponse Node { get; set; }

                [JsonProperty("media", NullValueHandling = NullValueHandling.Ignore)]
                public MediaResponse[] Media { get; set; }
            }

            internal class MediaResponse
            {
                [JsonProperty("id")]
                public int Id { get; set; }

                [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
                public Title Title { get; set; }
            }

            internal class NodeResponse
            {
                [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
                public Name Name { get; set; }

                [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
                public Image Image { get; set; }

                [JsonProperty("description")]
                public string Description { get; set; }
            }

        }
    }
}
