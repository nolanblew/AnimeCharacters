using Kitsu.Controllers;
using Kitsu.Responses;
using System;

namespace Kitsu.Models
{
    public class LibraryEntrySlim
    {
        public LibraryEntrySlim() { }

        public LibraryEntrySlim(UserLibraryEventGetResponse.IncludedItems includedLibraryItems, LibraryType type)
        {
            Id = includedLibraryItems.Id;
            Type = type;
            Status = (LibraryStatus)Enum.Parse(typeof(LibraryStatus), includedLibraryItems.Attributes.Status, true);
            IsReconsuming = includedLibraryItems.Attributes.Reconsuming.Value;
            StartedAt = includedLibraryItems.Attributes.StartedAt;
            FinishedAt = includedLibraryItems.Attributes.FinishedAt;
            ProgressedAt = includedLibraryItems.Attributes.ProgressedAt;
            Progress = includedLibraryItems.Attributes.Progress;
        }

        public long Id { get; set; }
        public LibraryType Type { get; set; }
        public LibraryStatus Status { get; set; }
        public long? MangaId { get; set; }
        public bool IsReconsuming { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public DateTimeOffset? ProgressedAt { get; set; }
        public long? Progress { get; set; }
    }

    public class LibraryEntry : LibraryEntrySlim
    {
        public LibraryEntry() { }

        public LibraryEntry(UserLibraryGetRequest.UserLibraryGet libraryResponse, LibraryType type)
        {
            Id = libraryResponse.Id ?? 0;
            Type = type;
            Status = (LibraryStatus)Enum.Parse(typeof(LibraryStatus), libraryResponse.Attributes.Status, true);
            AnimeId = libraryResponse.Relationships.Anime.Data?.Id ?? 0; 
            IsReconsuming = libraryResponse.Attributes.Reconsuming;
            StartedAt = libraryResponse.Attributes.StartedAt;
            FinishedAt = libraryResponse.Attributes.FinishedAt;
            ProgressedAt = libraryResponse.Attributes.ProgressedAt;
            Progress = libraryResponse.Attributes.Progress;
        }

        public LibraryEntry(
            UserLibraryGetRequest.UserLibraryGet libraryResponse,
            LibraryType type,
            string animeId,
            string myAnimeListId,
            string anilistId,
            UserLibraryGetRequest.IncludedAttributes includedAttributes)
             : this(libraryResponse, type)
        {
            Anime = new Anime(animeId, myAnimeListId, anilistId, includedAttributes);
        }

        public long? AnimeId { get; set; }
        public Anime Anime { get; set; }
    }
}
