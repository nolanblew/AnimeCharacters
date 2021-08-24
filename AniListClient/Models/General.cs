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
}
