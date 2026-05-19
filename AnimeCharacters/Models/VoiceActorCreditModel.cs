using System;
using System.Collections.Generic;

namespace AnimeCharacters.Models
{
    public class VoiceActorCreditModel
    {
        public string ExtensionId { get; set; }
        public string ExtensionName { get; set; }
        public string CategoryName { get; set; }
        public string MediaId { get; set; }
        public string MediaTitle { get; set; }
        public string MediaImageUrl { get; set; }
        public string MediaRoute { get; set; }
        public string CharacterId { get; set; }
        public string CharacterName { get; set; }
        public string CharacterImageUrl { get; set; }
        public string CharacterRoute { get; set; }
        public string ExternalUrl { get; set; }
        public DateTimeOffset? LastProgressedAt { get; set; }
    }

    public class VoiceActorCreditSection
    {
        public string CategoryName { get; set; }
        public string ExtensionName { get; set; }
        public IReadOnlyList<VoiceActorCreditModel> Credits { get; set; } = new List<VoiceActorCreditModel>();
    }
}
