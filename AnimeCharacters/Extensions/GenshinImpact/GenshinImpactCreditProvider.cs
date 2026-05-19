using AnimeCharacters.Models;
using AniListClient.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnimeCharacters.Extensions.GenshinImpact
{
    public class GenshinImpactCreditProvider : IVoiceActorCreditProvider
    {
        readonly IGenshinImpactCharacterRepository _characterRepository;

        public GenshinImpactCreditProvider(IGenshinImpactCharacterRepository characterRepository)
        {
            _characterRepository = characterRepository;
        }

        public string ExtensionId => BuiltInExtensionIds.GenshinImpact;

        public async Task<IReadOnlyList<VoiceActorCreditModel>> GetCreditsAsync(Staff staff)
        {
            var characters = await _characterRepository.GetCharactersAsync();

            return GenshinImpactCreditMapper.CreateCredits(staff, characters);
        }
    }
}
