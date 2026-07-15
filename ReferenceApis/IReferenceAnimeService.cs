using AniListClient.Models;
using Kitsu.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReferenceApis
{
    public interface IReferenceAnimeService
    {
        IEnumerable<ReferenceAnimeKey> GetKnownAnimeKeys(Anime anime);

        Task<ReferenceMediaResult> GetMediaWithCharactersAsync(
            Anime anime,
            IReadOnlyCollection<string> searchTitles);

        Task<Staff> GetStaffByIdAsync(string providerName, string id, string fallbackName = null);
    }
}
