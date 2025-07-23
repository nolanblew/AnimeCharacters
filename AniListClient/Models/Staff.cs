using System.Collections.Generic;

namespace AniListClient.Models
{
    public record Staff(
        int Id,
        Names Name,
        Language Language,
        Images Images,
        string Description,
        int? Age,
        DateOfBirth DateOfBirth,
        string BloodType,
        string SiteUrl,
        List<Character> Characters);

    public record DateOfBirth(
        int? Year,
        int? Month,
        int? Day);

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
