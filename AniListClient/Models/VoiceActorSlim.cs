namespace AniListClient.Models
{
    public record VoiceActorSlim(
        int Id,
        Names Name,
        string ProviderName = "anilist",
        string SiteUrl = null);
}
