using Kitsu.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Kitsu.Comparers
{
    public class LibraryComparer : IEqualityComparer<LibraryEntry>
    {
        public static LibraryComparer Default { get; } = new LibraryComparer();

        static IEqualityComparer<Anime> _AnimeComparer = ProjectionEqualityComparer.Create<Anime, string>(a => a.KitsuId);

        public bool Equals(LibraryEntry x, LibraryEntry y) =>
            y != null &&
            x.Id == y.Id &&
            x.Type == y.Type &&
            x.Status == y.Status &&
            x.MangaId == y.MangaId &&
            x.IsReconsuming == y.IsReconsuming &&
            EqualityComparer<DateTimeOffset?>.Default.Equals(x.StartedAt, y.StartedAt) &&
            EqualityComparer<DateTimeOffset?>.Default.Equals(x.FinishedAt, y.FinishedAt) &&
            EqualityComparer<DateTimeOffset?>.Default.Equals(x.ProgressedAt, y.ProgressedAt) &&
            x.Progress == y.Progress &&
            x.AnimeId == y.AnimeId &&
            _AnimeComparer.Equals(x.Anime, y.Anime);

        public int GetHashCode([DisallowNull] LibraryEntry obj)
        {
            HashCode hash = new();
            hash.Add(obj.Id);
            hash.Add(obj.Type);
            hash.Add(obj.Status);
            hash.Add(obj.MangaId);
            hash.Add(obj.IsReconsuming);
            hash.Add(obj.StartedAt);
            hash.Add(obj.FinishedAt);
            hash.Add(obj.ProgressedAt);
            hash.Add(obj.Progress);
            hash.Add(obj.AnimeId);
            hash.Add(obj.Anime);
            return hash.ToHashCode();
        }
    }
}
