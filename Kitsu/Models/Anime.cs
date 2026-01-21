using System;
using System.Collections.Generic;

namespace Kitsu.Models
{
    public class Anime
    {
        public Anime() { }

        public Anime(
            string kitsuId,
            string myAnimeListId,
            string anilistId,
            Responses.UserLibraryGetRequest.IncludedAttributes animeResponse)
        {
            KitsuId = kitsuId;
            MyAnimeListId = myAnimeListId;
            AnilistId = anilistId;
            Title = animeResponse.CanonicalTitle;
            RomanjiTitle = animeResponse.Titles?.EnJp;
            EnglishTitle = animeResponse.Titles?.En ?? animeResponse.Titles?.EnUs;
            KitsuSlug = animeResponse.Slug;
            PosterImageUrl = GetPosterImageUrl(animeResponse.PosterImage, Title);
            YoutubeId = animeResponse.YoutubeVideoId;
            ShowType = animeResponse.ShowType.HasValue ? ParseAnimeType(animeResponse.ShowType.Value) : null;

            if (string.IsNullOrWhiteSpace(RomanjiTitle) && string.IsNullOrWhiteSpace(EnglishTitle))
            {
                Console.WriteLine($"WARNING: Anime with Kitsu ID '{kitsuId}' is missing both EnJp and En/EnUs titles.");
            }
        }

        public Anime(
            string kistuId,
            string myAnimeListId,
            string anilistId,
            string title,
            string romanjiTitle,
            string synonymTitle,
            string kitsuSlug,
            string posterImageUrl,
            string youtubeId,
            AnimeType? showType)
        {
            KitsuId = kistuId;
            MyAnimeListId = myAnimeListId;
            AnilistId = anilistId;
            Title = title;
            RomanjiTitle = romanjiTitle;
            EnglishTitle = synonymTitle;
            KitsuSlug = kitsuSlug;
            PosterImageUrl = posterImageUrl;
            YoutubeId = youtubeId;
            ShowType = showType;
        }

        public string KitsuId { get; set; }
        public string MyAnimeListId { get; set; }
        public string AnilistId { get; set; }

        public string Title { get; set; }
        public string RomanjiTitle { get; set; }
        public string EnglishTitle { get; set; }
        public string PosterImageUrl { get; set; }
        public string KitsuSlug { get; set; }
        public string YoutubeId { get; set; }
        public AnimeType? ShowType { get; set; }

        public List<Anime> RelatedAnime { get; } = new List<Anime>();

        public static AnimeType? ParseAnimeType(Responses.UserLibraryGetRequest.ShowTypeEnum showType)
        {
            switch (showType)
            {
                case Responses.UserLibraryGetRequest.ShowTypeEnum.TV:
                    return AnimeType.Show;
                case Responses.UserLibraryGetRequest.ShowTypeEnum.Movie:
                    return AnimeType.Movie;
                case Responses.UserLibraryGetRequest.ShowTypeEnum.OVA:
                case Responses.UserLibraryGetRequest.ShowTypeEnum.Special:
                    return AnimeType.OVA;
            }

            return null;
        }

        private static string GetPosterImageUrl(Responses.UserLibraryGetRequest.PosterImage posterImage, string title)
        {
            var url = posterImage?.Large?.AbsoluteUri
                ?? posterImage?.Medium?.AbsoluteUri
                ?? posterImage?.Original?.AbsoluteUri
                ?? posterImage?.Small?.AbsoluteUri;

            if (!string.IsNullOrWhiteSpace(url))
            {
                return url;
            }

            return GeneratePlaceholderDataUri(title);
        }

        private static string GeneratePlaceholderDataUri(string title)
        {
            var initial = !string.IsNullOrWhiteSpace(title) ? title[0].ToString().ToUpperInvariant() : "?";
            var svg = $@"<svg xmlns='http://www.w3.org/2000/svg' width='225' height='350'>
  <defs>
    <linearGradient id='grad' x1='0%' y1='0%' x2='100%' y2='100%'>
      <stop offset='0%' style='stop-color:#4a4a4a;stop-opacity:1' />
      <stop offset='100%' style='stop-color:#2a2a2a;stop-opacity:1' />
    </linearGradient>
  </defs>
  <rect width='225' height='350' fill='url(#grad)' rx='4'/>
  <text x='50%' y='50%' dominant-baseline='middle' text-anchor='middle' font-family='Arial, sans-serif' font-size='80' font-weight='bold' fill='#888888'>{initial}</text>
</svg>";
            var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svg));
            return $"data:image/svg+xml;base64,{base64}";
        }

        public static AnimeType? ParseAnimeType(string animeType)
        {
            if (string.Equals(animeType, "TV", StringComparison.OrdinalIgnoreCase))
            {
                return AnimeType.Show;
            }
            if (string.Equals(animeType, "Movie", StringComparison.OrdinalIgnoreCase))
            {
                return AnimeType.Movie;
            }

            return AnimeType.OVA;
        }
    }

    public enum AnimeType
    {
        Show = 0,
        Movie = 1,
        OVA = 2,
    }
}
