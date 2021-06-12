using Kitsu.Controllers;
using Kitsu.Responses;
using System;

namespace Kitsu.Models
{
    public class LibraryEntry
    {
        public LibraryEntry(UserLibraryGetRequest.UserLibraryGet libraryResponse, LibraryType type)
        {
            Id = (int?)libraryResponse.Id ?? 0;
            Type = type;
            Status = (LibraryStatus)Enum.Parse(typeof(LibraryStatus), libraryResponse.Attributes.Status, true);
            AnimeId = libraryResponse.Relationships.Anime.Data?.Id ?? 0;
        }

        public LibraryEntry(UserLibraryGetRequest.UserLibraryGet libraryResponse, LibraryType type, string animeId, string myAnimeListId, Responses.UserLibraryGetRequest.IncludedAttributes includedAttributes)
             : this(libraryResponse, type)
        {
            Anime = new Anime(animeId, myAnimeListId, includedAttributes);
        }

        public LibraryEntry(int id, LibraryType type, LibraryStatus status, long? animeId = null, long? mangaId = null, Anime anime = null)
        {
            Id = id;
            Type = type;
            Status = status;
            AnimeId = animeId;
            MangaId = mangaId;
            Anime = anime;
        }

        public int Id { get; set; }
        public LibraryType Type { get; set; }
        public LibraryStatus Status { get; set; }
        public long? AnimeId { get; set; }
        public long? MangaId { get; set; }

        public Anime Anime { get; set; }
    }
}
