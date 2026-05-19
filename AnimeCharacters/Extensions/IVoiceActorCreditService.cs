using AnimeCharacters.Models;
using AniListClient.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnimeCharacters.Extensions
{
    public interface IVoiceActorCreditService
    {
        Task<IReadOnlyList<VoiceActorCreditSection>> GetCreditSectionsAsync(Staff staff);
    }
}
