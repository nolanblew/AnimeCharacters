using AniListClient.Models;
using Kitsu.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReferenceApis
{
    public interface IReferenceAnimeProvider
    {
        string Name { get; }
        string DisplayName { get; }

        ReferenceAnimeKey GetKnownAnimeKey(Anime anime);

        Task<ReferenceMediaResult> GetMediaWithCharactersAsync(
            Anime anime,
            IReadOnlyCollection<string> searchTitles);

        Task<Staff> GetStaffByIdAsync(string id);

        Task<Staff> FindStaffByNameAsync(string name);
    }
}
