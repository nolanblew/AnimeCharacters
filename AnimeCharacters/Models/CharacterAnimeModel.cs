using System;

namespace AnimeCharacters.Models
{
    public class CharacterAnimeModel
    {
        public string KitsuId { get; set; }
        public string AnimeImageUrl { get; set; }
        public DateTimeOffset? LastProgressedAt { get; set; }
        public JikanDotNet.VoiceActingRole VoiceActingRole { get; set; }
    }
}
