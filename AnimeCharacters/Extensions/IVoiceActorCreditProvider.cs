using AnimeCharacters.Models;
using AniListClient.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnimeCharacters.Extensions
{
    /// <summary>
    /// Supplies credits for one media extension on a voice actor page.
    /// </summary>
    public interface IVoiceActorCreditProvider
    {
        string ExtensionId { get; }
        Task<IReadOnlyList<VoiceActorCreditModel>> GetCreditsAsync(Staff staff);
    }
}
