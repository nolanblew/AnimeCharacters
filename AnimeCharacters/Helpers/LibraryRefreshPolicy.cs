using Kitsu.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnimeCharacters.Helpers
{
    /// <summary>Keeps anime metadata fresh independently of watch-progress synchronization.</summary>
    public static class LibraryRefreshPolicy
    {
        /// <summary>Refreshes metadata daily, on request, and for caches without metadata timestamps.</summary>
        public static bool RequiresFullRefresh(IEnumerable<LibraryEntry> entries, bool manualRefresh,
            bool isMigrating, DateTimeOffset now)
        {
            return manualRefresh || isMigrating || !entries.Any()
                || entries.Any(entry => entry.Anime == null
                    || !entry.Anime.MetadataFetchedAt.HasValue
                    || now - entry.Anime.MetadataFetchedAt.Value >= TimeSpan.FromDays(1));
        }
    }
}
