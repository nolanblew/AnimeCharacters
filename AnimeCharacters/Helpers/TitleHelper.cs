using AnimeCharacters.Models;
using AniListClient.Models;

namespace AnimeCharacters.Helpers
{
    public static class TitleHelper
    {
        public static string GetPreferredTitle(Kitsu.Models.Anime kitsuAnime, TitleType preferredType)
        {
            if (kitsuAnime == null) return "Loading...";

            return preferredType switch
            {
                TitleType.English => kitsuAnime.EnglishTitle ?? kitsuAnime.Title ?? "Unknown Title",
                TitleType.Romaji => kitsuAnime.RomanjiTitle ?? kitsuAnime.Title ?? "Unknown Title",
                TitleType.Native => kitsuAnime.Title ?? "Unknown Title", // Kitsu doesn't have native Japanese, fallback to canonical
                TitleType.UserPreferred => kitsuAnime.EnglishTitle ?? kitsuAnime.Title ?? "Unknown Title", // Default to English for Kitsu
                _ => kitsuAnime.Title ?? "Unknown Title"
            };
        }

        public static string GetPreferredTitle(AniListClient.Models.Titles anilistTitles, TitleType preferredType)
        {
            if (anilistTitles == null) return "Loading...";

            return preferredType switch
            {
                TitleType.English => anilistTitles.English ?? anilistTitles.UserPreferred ?? anilistTitles.Romaji ?? "Unknown Title",
                TitleType.Romaji => anilistTitles.Romaji ?? anilistTitles.UserPreferred ?? anilistTitles.English ?? "Unknown Title",
                TitleType.Native => anilistTitles.Native ?? anilistTitles.UserPreferred ?? anilistTitles.Romaji ?? "Unknown Title",
                TitleType.UserPreferred => anilistTitles.UserPreferred ?? anilistTitles.English ?? anilistTitles.Romaji ?? "Unknown Title",
                _ => anilistTitles.UserPreferred ?? anilistTitles.English ?? anilistTitles.Romaji ?? "Unknown Title"
            };
        }

        public static string GetPreferredTitle(AniListClient.Models.Media anilistMedia, TitleType preferredType)
        {
            return GetPreferredTitle(anilistMedia?.Title, preferredType);
        }

        public static string GetPreferredTitle(AniListClient.Models.MediaBase anilistMediaBase, TitleType preferredType)
        {
            return GetPreferredTitle(anilistMediaBase?.Title, preferredType);
        }
    }
}