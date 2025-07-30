using AnimeCharacters.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeCharacters.Services
{
    /// <summary>
    /// Default implementation of data provider service that manages multiple providers
    /// </summary>
    public class DataProviderService : IDataProviderService
    {
        private readonly List<IDataProvider> _providers = new();

        public void RegisterProvider<T>(T provider) where T : IDataProvider
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Remove any existing provider with the same ID
            _providers.RemoveAll(p => p.ProviderId == provider.ProviderId);
            _providers.Add(provider);
        }

        public List<T> GetProviders<T>() where T : IDataProvider
        {
            return _providers.OfType<T>().Where(p => p.IsEnabled).OrderByDescending(p => p.Priority).ToList();
        }

        public T GetPrimaryProvider<T>() where T : IDataProvider
        {
            return GetProviders<T>().FirstOrDefault();
        }

        public async Task<List<UnifiedLibraryEntry>> GetMergedUserLibraryAsync(string userId)
        {
            var providers = GetProviders<IAnimeLibraryProvider>();
            var allEntries = new List<UnifiedLibraryEntry>();

            foreach (var provider in providers)
            {
                try
                {
                    var entries = await provider.GetUserLibraryAsync(userId);
                    allEntries.AddRange(entries);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other providers
                    Console.WriteLine($"Error fetching library from provider {provider.ProviderId}: {ex.Message}");
                }
            }

            return MergeLibraryEntries(allEntries);
        }

        public async Task<List<UnifiedCharacter>> GetMergedCharactersForAnimeAsync(UnifiedAnime anime)
        {
            var providers = GetProviders<ICharacterDataProvider>();
            var allCharacters = new List<UnifiedCharacter>();

            foreach (var provider in providers)
            {
                try
                {
                    // Try to find the anime ID for this provider
                    if (anime.ProviderIds.TryGetValue(provider.ProviderId, out var animeId))
                    {
                        var characters = await provider.GetCharactersForAnimeAsync(animeId);
                        allCharacters.AddRange(characters);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching characters from provider {provider.ProviderId}: {ex.Message}");
                }
            }

            return MergeCharacters(allCharacters);
        }

        public async Task<UnifiedVoiceActor> GetMergedVoiceActorAsync(string voiceActorId, string providerId)
        {
            var providers = GetProviders<ICharacterDataProvider>();
            var voiceActors = new List<UnifiedVoiceActor>();

            foreach (var provider in providers)
            {
                try
                {
                    // If providerId is specified, only use that provider
                    if (!string.IsNullOrEmpty(providerId) && provider.ProviderId != providerId)
                        continue;

                    var voiceActor = await provider.GetVoiceActorAsync(voiceActorId);
                    if (voiceActor != null)
                        voiceActors.Add(voiceActor);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching voice actor from provider {provider.ProviderId}: {ex.Message}");
                }
            }

            return MergeVoiceActors(voiceActors).FirstOrDefault();
        }

        public async Task<UnifiedAnime> GetMergedAnimeAsync(string animeId, string providerId)
        {
            var providers = GetProviders<IAnimeMetadataProvider>();
            var animes = new List<UnifiedAnime>();

            foreach (var provider in providers)
            {
                try
                {
                    if (!string.IsNullOrEmpty(providerId) && provider.ProviderId != providerId)
                        continue;

                    var anime = await provider.GetAnimeAsync(animeId);
                    if (anime != null)
                        animes.Add(anime);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching anime from provider {provider.ProviderId}: {ex.Message}");
                }
            }

            return MergeAnimes(animes).FirstOrDefault();
        }

        private List<UnifiedLibraryEntry> MergeLibraryEntries(List<UnifiedLibraryEntry> entries)
        {
            // Group entries by anime (using cross-provider IDs)
            var groupedEntries = new Dictionary<string, List<UnifiedLibraryEntry>>();

            foreach (var entry in entries)
            {
                var key = GenerateAnimeKey(entry.Anime);
                if (!groupedEntries.ContainsKey(key))
                    groupedEntries[key] = new List<UnifiedLibraryEntry>();
                groupedEntries[key].Add(entry);
            }

            var mergedEntries = new List<UnifiedLibraryEntry>();
            foreach (var group in groupedEntries.Values)
            {
                mergedEntries.Add(MergeLibraryEntry(group));
            }

            return mergedEntries;
        }

        private UnifiedLibraryEntry MergeLibraryEntry(List<UnifiedLibraryEntry> entries)
        {
            if (!entries.Any()) return null;
            if (entries.Count == 1) return entries[0];

            // Use the entry from the highest priority provider as base
            var primaryEntry = entries.OrderByDescending(e => GetProviderPriority(e.ProviderId)).First();
            var merged = new UnifiedLibraryEntry
            {
                Id = primaryEntry.Id,
                Anime = MergeAnimes(entries.Select(e => e.Anime).ToList()).FirstOrDefault(),
                Status = primaryEntry.Status,
                Progress = primaryEntry.Progress,
                ProgressedAt = primaryEntry.ProgressedAt,
                StartedAt = primaryEntry.StartedAt,
                FinishedAt = primaryEntry.FinishedAt,
                IsReconsuming = primaryEntry.IsReconsuming,
                ProviderId = primaryEntry.ProviderId,
                OriginalData = primaryEntry.OriginalData
            };

            return merged;
        }

        private List<UnifiedAnime> MergeAnimes(List<UnifiedAnime> animes)
        {
            if (!animes.Any()) return new List<UnifiedAnime>();
            
            var groupedAnimes = new Dictionary<string, List<UnifiedAnime>>();

            foreach (var anime in animes)
            {
                var key = GenerateAnimeKey(anime);
                if (!groupedAnimes.ContainsKey(key))
                    groupedAnimes[key] = new List<UnifiedAnime>();
                groupedAnimes[key].Add(anime);
            }

            var mergedAnimes = new List<UnifiedAnime>();
            foreach (var group in groupedAnimes.Values)
            {
                mergedAnimes.Add(MergeAnime(group));
            }

            return mergedAnimes;
        }

        private UnifiedAnime MergeAnime(List<UnifiedAnime> animes)
        {
            if (!animes.Any()) return null;
            if (animes.Count == 1) return animes[0];

            var primaryAnime = animes.OrderByDescending(a => GetProviderPriority(a.ProviderId)).First();
            var merged = new UnifiedAnime
            {
                Id = primaryAnime.Id,
                Title = primaryAnime.Title,
                RomanjiTitle = primaryAnime.RomanjiTitle,
                EnglishTitle = primaryAnime.EnglishTitle,
                PosterImageUrl = primaryAnime.PosterImageUrl,
                Description = primaryAnime.Description,
                Type = primaryAnime.Type,
                ProviderId = primaryAnime.ProviderId,
                ProviderIds = MergeProviderIds(animes.Select(a => a.ProviderIds)),
                OriginalData = primaryAnime.OriginalData
            };

            return merged;
        }

        private List<UnifiedCharacter> MergeCharacters(List<UnifiedCharacter> characters)
        {
            var groupedCharacters = new Dictionary<string, List<UnifiedCharacter>>();

            foreach (var character in characters)
            {
                var key = GenerateCharacterKey(character);
                if (!groupedCharacters.ContainsKey(key))
                    groupedCharacters[key] = new List<UnifiedCharacter>();
                groupedCharacters[key].Add(character);
            }

            var mergedCharacters = new List<UnifiedCharacter>();
            foreach (var group in groupedCharacters.Values)
            {
                mergedCharacters.Add(MergeCharacter(group));
            }

            return mergedCharacters;
        }

        private UnifiedCharacter MergeCharacter(List<UnifiedCharacter> characters)
        {
            if (!characters.Any()) return null;
            if (characters.Count == 1) return characters[0];

            var primaryCharacter = characters.OrderByDescending(c => GetProviderPriority(c.ProviderId)).First();
            var merged = new UnifiedCharacter
            {
                Id = primaryCharacter.Id,
                Name = primaryCharacter.Name,
                RomanjiName = primaryCharacter.RomanjiName,
                EnglishName = primaryCharacter.EnglishName,
                ImageUrl = primaryCharacter.ImageUrl,
                Description = primaryCharacter.Description,
                Role = primaryCharacter.Role,
                VoiceActors = MergeVoiceActors(characters.SelectMany(c => c.VoiceActors).ToList()),
                ProviderId = primaryCharacter.ProviderId,
                ProviderIds = MergeProviderIds(characters.Select(c => c.ProviderIds)),
                OriginalData = primaryCharacter.OriginalData
            };

            return merged;
        }

        private List<UnifiedVoiceActor> MergeVoiceActors(List<UnifiedVoiceActor> voiceActors)
        {
            var groupedVoiceActors = new Dictionary<string, List<UnifiedVoiceActor>>();

            foreach (var va in voiceActors)
            {
                var key = GenerateVoiceActorKey(va);
                if (!groupedVoiceActors.ContainsKey(key))
                    groupedVoiceActors[key] = new List<UnifiedVoiceActor>();
                groupedVoiceActors[key].Add(va);
            }

            var mergedVoiceActors = new List<UnifiedVoiceActor>();
            foreach (var group in groupedVoiceActors.Values)
            {
                mergedVoiceActors.Add(MergeVoiceActor(group));
            }

            return mergedVoiceActors;
        }

        private UnifiedVoiceActor MergeVoiceActor(List<UnifiedVoiceActor> voiceActors)
        {
            if (!voiceActors.Any()) return null;
            if (voiceActors.Count == 1) return voiceActors[0];

            var primaryVA = voiceActors.OrderByDescending(va => GetProviderPriority(va.ProviderId)).First();
            var merged = new UnifiedVoiceActor
            {
                Id = primaryVA.Id,
                Name = primaryVA.Name,
                RomanjiName = primaryVA.RomanjiName,
                EnglishName = primaryVA.EnglishName,
                ImageUrl = primaryVA.ImageUrl,
                Description = primaryVA.Description,
                Age = primaryVA.Age,
                DateOfBirth = primaryVA.DateOfBirth,
                BloodType = primaryVA.BloodType,
                Language = primaryVA.Language,
                ProviderId = primaryVA.ProviderId,
                ProviderIds = MergeProviderIds(voiceActors.Select(va => va.ProviderIds)),
                OriginalData = primaryVA.OriginalData
                // Note: Characters list will be populated separately to avoid circular references
            };

            return merged;
        }

        private Dictionary<string, string> MergeProviderIds(IEnumerable<Dictionary<string, string>> providerIdsList)
        {
            var merged = new Dictionary<string, string>();
            foreach (var providerIds in providerIdsList)
            {
                foreach (var kvp in providerIds)
                {
                    merged[kvp.Key] = kvp.Value;
                }
            }
            return merged;
        }

        private string GenerateAnimeKey(UnifiedAnime anime)
        {
            // Use provider IDs for matching first, then fall back to title matching
            if (anime.ProviderIds.Any())
            {
                var sortedIds = anime.ProviderIds.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}:{kvp.Value}");
                return string.Join("|", sortedIds);
            }

            // Fallback to title-based matching
            var normalizedTitle = NormalizeTitle(anime.Title ?? anime.RomanjiTitle ?? anime.EnglishTitle);
            return $"title:{normalizedTitle}";
        }

        private string GenerateCharacterKey(UnifiedCharacter character)
        {
            if (character.ProviderIds.Any())
            {
                var sortedIds = character.ProviderIds.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}:{kvp.Value}");
                return string.Join("|", sortedIds);
            }

            var normalizedName = NormalizeTitle(character.Name ?? character.RomanjiName ?? character.EnglishName);
            return $"name:{normalizedName}";
        }

        private string GenerateVoiceActorKey(UnifiedVoiceActor voiceActor)
        {
            if (voiceActor.ProviderIds.Any())
            {
                var sortedIds = voiceActor.ProviderIds.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}:{kvp.Value}");
                return string.Join("|", sortedIds);
            }

            var normalizedName = NormalizeTitle(voiceActor.Name ?? voiceActor.RomanjiName ?? voiceActor.EnglishName);
            return $"name:{normalizedName}";
        }

        private string NormalizeTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return "";

            return title.ToLowerInvariant()
                       .Replace(" ", "")
                       .Replace("-", "")
                       .Replace("_", "")
                       .Replace(":", "")
                       .Replace("!", "")
                       .Replace("?", "");
        }

        private int GetProviderPriority(string providerId)
        {
            var provider = _providers.FirstOrDefault(p => p.ProviderId == providerId);
            return provider?.Priority ?? 0;
        }
    }
}