using AnimeCharacters.Models;
using AniListClient.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeCharacters.Extensions
{
    public class VoiceActorCreditService : IVoiceActorCreditService
    {
        readonly IDatabaseProvider _databaseProvider;
        readonly IEnumerable<IVoiceActorCreditProvider> _creditProviders;

        public VoiceActorCreditService(
            IDatabaseProvider databaseProvider,
            IEnumerable<IVoiceActorCreditProvider> creditProviders)
        {
            _databaseProvider = databaseProvider;
            _creditProviders = creditProviders;
        }

        public async Task<IReadOnlyList<VoiceActorCreditSection>> GetCreditSectionsAsync(Staff staff)
        {
            var settings = await _databaseProvider.GetUserSettingsAsync() ?? new UserSettings();
            var enabledExtensionIds = ExtensionCatalog.GetEnabledExtensions(settings)
                .Select(extension => extension.Id)
                .ToHashSet();

            var credits = new List<VoiceActorCreditModel>();

            foreach (var provider in _creditProviders.Where(provider => enabledExtensionIds.Contains(provider.ExtensionId)))
            {
                credits.AddRange(await provider.GetCreditsAsync(staff));
            }

            return credits
                .GroupBy(credit => new { credit.CategoryName, credit.ExtensionName })
                .Select(group => new VoiceActorCreditSection
                {
                    CategoryName = group.Key.CategoryName,
                    ExtensionName = group.Key.ExtensionName,
                    Credits = group.ToList()
                })
                .OrderBy(section => section.CategoryName == "Anime" ? 0 : 1)
                .ThenBy(section => section.CategoryName)
                .ThenBy(section => section.ExtensionName)
                .ToList();
        }
    }
}
