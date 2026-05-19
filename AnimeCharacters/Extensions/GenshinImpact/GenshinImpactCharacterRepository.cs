using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AnimeCharacters.Extensions.GenshinImpact
{
    public interface IGenshinImpactCharacterRepository
    {
        Task<IReadOnlyList<GenshinImpactCharacter>> GetCharactersAsync();
    }

    public class GenshinImpactCharacterRepository : IGenshinImpactCharacterRepository
    {
        const string _DATA_PATH = "data/extensions/genshin-impact-characters.json";

        readonly HttpClient _httpClient;
        Task<IReadOnlyList<GenshinImpactCharacter>> _charactersTask;

        public GenshinImpactCharacterRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<IReadOnlyList<GenshinImpactCharacter>> GetCharactersAsync() =>
            _charactersTask ??= LoadCharactersAsync();

        async Task<IReadOnlyList<GenshinImpactCharacter>> LoadCharactersAsync() =>
            await _httpClient.GetFromJsonAsync<List<GenshinImpactCharacter>>(_DATA_PATH) ??
            new List<GenshinImpactCharacter>();
    }
}
