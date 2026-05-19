using System.Collections.Generic;
using System.Linq;

namespace AnimeCharacters.Extensions.GenshinImpact
{
    public class GenshinImpactCharacter
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string WikiUrl { get; set; }
        public string JapaneseVoiceActorName { get; set; }
        public string JapaneseVoiceActorNativeName { get; set; }
        public int? AniListStaffId { get; set; }
        public List<GenshinImpactVoiceActor> JapaneseVoiceActors { get; set; } = new();

        public IReadOnlyList<GenshinImpactVoiceActor> GetJapaneseVoiceActors()
        {
            if (JapaneseVoiceActors?.Any() == true)
            {
                return JapaneseVoiceActors;
            }

            if (string.IsNullOrWhiteSpace(JapaneseVoiceActorName) && string.IsNullOrWhiteSpace(JapaneseVoiceActorNativeName))
            {
                return new List<GenshinImpactVoiceActor>();
            }

            return new List<GenshinImpactVoiceActor>
            {
                new()
                {
                    Name = JapaneseVoiceActorName,
                    NativeName = JapaneseVoiceActorNativeName,
                    AniListStaffId = AniListStaffId
                }
            };
        }
    }

    public class GenshinImpactVoiceActor
    {
        public string Name { get; set; }
        public string NativeName { get; set; }
        public int? AniListStaffId { get; set; }
    }
}
