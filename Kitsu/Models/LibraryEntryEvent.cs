using Kitsu.Responses;
using System;
using System.Linq;

namespace Kitsu.Models
{
    public class LibraryEntryEvent
    {
        public LibraryEntryEvent() { }

        public LibraryEntryEvent(UserLibraryEventGetResponse.Datum response, UserLibraryEventGetResponse.IncludedItems libraryEntry)
        {
            Id = response.Id;
            CreatedAt = response.Attributes.CreatedAt;
            EventKind = response.Attributes.Kind;
            LibraryEntryId = libraryEntry.Id;
            Type = _DetermineEventType(EventKind.Value, response.Attributes.ChangedData);

            if (Type != EventType.Removed)
            {
                LibraryEntrySlim = new(libraryEntry, Controllers.LibraryType.Anime);
            }
        }

        public long? Id { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public UserLibraryEventGetResponse.Kind? EventKind { get; set; }

        public long? LibraryEntryId { get; set; }

        public LibraryEntrySlim LibraryEntrySlim { get; set; }

        public EventType Type { get; set; }

        public enum EventType
        {
            Updated,
            Watching,
            Completed,
            Removed,
        }

        EventType _DetermineEventType(UserLibraryEventGetResponse.Kind kind, UserLibraryEventGetResponse.ChangedData changeData = null)
        {
            if (kind == UserLibraryEventGetResponse.Kind.Progressed)
            {
                return EventType.Updated;
            }

            if (changeData == null) { return EventType.Updated; }

            return changeData.Status.LastOrDefault() switch
            {
                "current" => EventType.Watching,
                "completed" => EventType.Completed,
                _ => EventType.Removed,
            };
        }

    }
}
