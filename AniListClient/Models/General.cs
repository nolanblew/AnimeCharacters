namespace AniListClient.Models
{
    public record Titles(
        string Romaji,
        string English,
        string Native,
        string UserPreferred);

    public record Images(
        string Medium,
        string Large,
        string ExtraLarge,
        string Color)
    {
        public string GetOptimalImage() =>
            !string.IsNullOrWhiteSpace(Large) ? Large :
            !string.IsNullOrWhiteSpace(ExtraLarge) ? ExtraLarge :
            !string.IsNullOrWhiteSpace(Medium) ? Medium :
            null;
    }

    public record FuzzyDate(
        int? Year,
        int? Month,
        int? Day)
    {
        public override string ToString() =>
            Month.HasValue && Day.HasValue
                ? $"{Month.Value:D2}/{Day.Value:D2}{(Year.HasValue ? $"/{Year.Value}" : string.Empty)}"
                : Year?.ToString() ?? string.Empty;
    }
}
