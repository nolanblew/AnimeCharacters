using Kitsu.Controllers;
using Kitsu.Responses;
using System;

namespace Kitsu.Models
{
    public class LibraryEntry
    {
        public LibraryEntry() { }

        public LibraryEntry(UserLibraryGetRequest.UserLibraryGet libraryResponse, LibraryType type)
        {
            Id = (int?)libraryResponse.Id ?? 0;
            Type = type;
            Status = (LibraryStatus)Enum.Parse(typeof(LibraryStatus), libraryResponse.Attributes.Status, true);
            AnimeId = libraryResponse.Relationships.Anime.Data?.Id ?? 0;
            IsReconsuming = libraryResponse.Attributes.Reconsuming;
            StartedAt = libraryResponse.Attributes.StartedAt;
            FinishedAt = libraryResponse.Attributes.FinishedAt;
            ProgressedAt = libraryResponse.Attributes.ProgressedAt;
            Progress = libraryResponse.Attributes.Progress;
        }

        public LibraryEntry(UserLibraryGetRequest.UserLibraryGet libraryResponse, LibraryType type, string animeId, string myAnimeListId, Responses.UserLibraryGetRequest.IncludedAttributes includedAttributes)
             : this(libraryResponse, type)
        {
            Anime = new Anime(animeId, myAnimeListId, includedAttributes);
        }
        public int Id { get; set; }
        public LibraryType Type { get; set; }
        public LibraryStatus Status { get; set; }
        public long? AnimeId { get; set; }
        public long? MangaId { get; set; }
        public bool IsReconsuming { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public DateTimeOffset? ProgressedAt { get; set; }
        public long? Progress { get; set; }

        public Anime Anime { get; set; }
    }
}
