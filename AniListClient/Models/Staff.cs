using System.Collections.Generic;

namespace AniListClient.Models
{
    public record Staff(
        int Id,
        Names Name,
        Language Language,
        Images Images,
        string Description,
        List<Character> Characters);

    public enum Language
    {
        Japanese,
        English,
        Korean,
        Italian,
        Spanish,
        Portuguese,
        French,
        German,
        Hebrew,
        Hungarian,
        Chinese,
        Arabic,
        Filipino,
        Catalan
    }
}
