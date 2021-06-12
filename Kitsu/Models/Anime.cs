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
            Responses.UserLibraryGetRequest.IncludedAttributes animeResponse)
        {
            KitsuId = kitsuId;
            MyAnimeListId = myAnimeListId;
            Title = animeResponse.CanonicalTitle;
            RomanjiTitle = animeResponse.Titles.EnJp;
            EnglishTitle = animeResponse.Titles.En;
            KitsuSlug = animeResponse.Slug;
            PosterImageUrl = animeResponse.PosterImage.Large.AbsoluteUri;
            YoutubeId = animeResponse.YoutubeVideoId;
            ShowType = animeResponse.ShowType.HasValue ? ParseAnimeType(animeResponse.ShowType.Value) : null;
        }

        public Anime(
            string kistuId,
            string myAnimeListId,
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
