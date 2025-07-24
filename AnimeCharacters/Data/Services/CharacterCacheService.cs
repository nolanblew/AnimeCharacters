using AniListClient.Models;
using AnimeCharacters.Data.Extensions;
using AnimeCharacters.Data.Models;
using AnimeCharacters.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeCharacters.Data.Services
{
    public class CharacterCacheService : ICharacterCacheService
    {
        private readonly IDbContextFactory<AnimeCharactersDbContext> _contextFactory;
        private readonly AniListClient.AniListClient _aniListClient;

        public CharacterCacheService(
            IDbContextFactory<AnimeCharactersDbContext> contextFactory,
            AniListClient.AniListClient aniListClient)
        {
            _contextFactory = contextFactory;
            _aniListClient = aniListClient;
        }

        public async Task<List<Character>> GetCharactersForAnimeAsync(string kitsuId, string anilistId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // First, try to get from cache
            var cachedCharacters = await context.AnimeCharacters
                .Where(ac => ac.AnimeKitsuId == kitsuId)
                .Include(ac => ac.Character)
                .ThenInclude(c => c.CharacterVoiceActors)
                .ThenInclude(cva => cva.VoiceActor)
                .Select(ac => ac.Character)
                .ToListAsync();

            if (cachedCharacters.Any())
            {
                return cachedCharacters.Select(dc => dc.ToCharacter()).ToList();
            }

            // If not in cache and we have AniList ID, fetch from AniList
            if (!string.IsNullOrEmpty(anilistId) && int.TryParse(anilistId, out var anilistIdInt))
            {
                try
                {
                    var media = await _aniListClient.Characters.GetMediaWithCharactersById(anilistIdInt);
                    if (media?.Characters != null)
                    {
                        // Cache the data
                        await CacheCharacterDataAsync(kitsuId, media.Characters);
                        return media.Characters;
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't fail - return empty list
                    Console.WriteLine($"Failed to fetch characters for anime {kitsuId}: {ex.Message}");
                }
            }

            return new List<Character>();
        }

        public async Task<Character> GetCharacterAsync(int characterId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var cachedCharacter = await context.Characters
                .Include(c => c.CharacterVoiceActors)
                .ThenInclude(cva => cva.VoiceActor)
                .FirstOrDefaultAsync(c => c.Id == characterId);

            if (cachedCharacter != null)
            {
                return cachedCharacter.ToCharacter();
            }

            // If not in cache, try to fetch from AniList
            try
            {
                // Note: The current AniListClient doesn't have individual character lookup
                // This would need to be implemented in the AniListClient
                Character character = null; // await _aniListClient.Characters.GetCharacterById(characterId);
                if (character != null)
                {
                    // Cache the character
                    var dbCharacter = character.ToDbCharacter();
                    context.Characters.Add(dbCharacter);

                    // Cache voice actors and relationships
                    foreach (var voiceActor in character.VoiceActors)
                    {
                        var existingVA = await context.VoiceActors.FirstOrDefaultAsync(va => va.Id == voiceActor.Id);
                        if (existingVA == null)
                        {
                            context.VoiceActors.Add(new DbVoiceActor
                            {
                                Id = voiceActor.Id,
                                NameRomaji = voiceActor.Name.Romaji,
                                NameFirst = voiceActor.Name.First,
                                NameLast = voiceActor.Name.Last,
                                NameFull = voiceActor.Name.Full,
                                NameNative = voiceActor.Name.Native,
                                NameAlternative = voiceActor.Name.Alternative,
                                NameAlternativeSpoiler = voiceActor.Name.AlternativeSpoiler
                            });
                        }

                        context.CharacterVoiceActors.Add(new DbCharacterVoiceActor
                        {
                            CharacterId = character.Id,
                            VoiceActorId = voiceActor.Id
                        });
                    }

                    await context.SaveChangesAsync();
                    return character;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch character {characterId}: {ex.Message}");
            }

            return null;
        }

        public async Task<Staff> GetVoiceActorAsync(int voiceActorId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var cachedVoiceActor = await context.VoiceActors
                .FirstOrDefaultAsync(va => va.Id == voiceActorId);

            if (cachedVoiceActor != null)
            {
                return cachedVoiceActor.ToStaff();
            }

            // If not in cache, try to fetch from AniList
            try
            {
                var staff = await _aniListClient.Staff.GetStaffById(voiceActorId);
                if (staff != null)
                {
                    // Cache the voice actor
                    context.VoiceActors.Add(staff.ToDbVoiceActor());
                    await context.SaveChangesAsync();
                    return staff;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch voice actor {voiceActorId}: {ex.Message}");
            }

            return null;
        }

        public async Task<List<CharacterAnimeModel>> GetSharedCharacterAnimesAsync(int voiceActorId, List<string> userAnimeKitsuIds)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var sharedAnimes = await context.CharacterVoiceActors
                .Where(cva => cva.VoiceActorId == voiceActorId)
                .Join(context.AnimeCharacters,
                    cva => cva.CharacterId,
                    ac => ac.CharacterId,
                    (cva, ac) => new { cva, ac })
                .Where(joined => userAnimeKitsuIds.Contains(joined.ac.AnimeKitsuId))
                .Include(joined => joined.cva.Character)
                .Include(joined => joined.ac.Anime)
                .Select(joined => new CharacterAnimeModel
                {
                    KitsuId = joined.ac.AnimeKitsuId,
                    AnimeImageUrl = joined.ac.Anime.PosterImageUrl,
                    LastProgressedAt = context.LibraryEntries
                        .Where(le => le.AnimeKitsuId == joined.ac.AnimeKitsuId)
                        .Select(le => le.ProgressedAt)
                        .FirstOrDefault(),
                    VoiceActingRole = joined.cva.Character.ToCharacter()
                })
                .ToListAsync();

            return sharedAnimes;
        }

        public async Task PreCacheUserAnimeCharactersAsync(List<string> animeKitsuIds)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            foreach (var kitsuId in animeKitsuIds)
            {
                // Check if already cached
                var isCached = await context.AnimeCharacters
                    .AnyAsync(ac => ac.AnimeKitsuId == kitsuId);

                if (!isCached)
                {
                    // Get the anime's AniList ID
                    var anime = await context.Anime
                        .FirstOrDefaultAsync(a => a.KitsuId == kitsuId);

                    if (anime != null && !string.IsNullOrEmpty(anime.AnilistId) && 
                        int.TryParse(anime.AnilistId, out var anilistId))
                    {
                        try
                        {
                            var media = await _aniListClient.Characters.GetMediaWithCharactersById(anilistId);
                            if (media?.Characters != null)
                            {
                                await CacheCharacterDataAsync(kitsuId, media.Characters);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to pre-cache characters for anime {kitsuId}: {ex.Message}");
                        }
                    }
                }
            }
        }

        public async Task ClearCacheAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            context.CharacterVoiceActors.RemoveRange(context.CharacterVoiceActors);
            context.AnimeCharacters.RemoveRange(context.AnimeCharacters);
            context.VoiceActors.RemoveRange(context.VoiceActors);
            context.Characters.RemoveRange(context.Characters);

            await context.SaveChangesAsync();
        }

        public async Task<CharacterCacheStats> GetCacheStatsAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var stats = new CharacterCacheStats
            {
                TotalCharacters = await context.Characters.CountAsync(),
                TotalVoiceActors = await context.VoiceActors.CountAsync(),
                TotalAnimeCharacterRelations = await context.AnimeCharacters.CountAsync(),
                CachedAnimesCount = await context.AnimeCharacters
                    .Select(ac => ac.AnimeKitsuId)
                    .Distinct()
                    .CountAsync(),
                LastCacheUpdate = await context.Characters
                    .OrderByDescending(c => c.UpdatedAt)
                    .Select(c => c.UpdatedAt)
                    .FirstOrDefaultAsync()
            };

            return stats;
        }

        private async Task CacheCharacterDataAsync(string animeKitsuId, List<Character> characters)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            foreach (var character in characters)
            {
                // Cache character if not exists
                var existingChar = await context.Characters.FirstOrDefaultAsync(c => c.Id == character.Id);
                if (existingChar == null)
                {
                    context.Characters.Add(character.ToDbCharacter());
                }

                // Cache anime-character relationship
                var existingAnimeChar = await context.AnimeCharacters
                    .FirstOrDefaultAsync(ac => ac.AnimeKitsuId == animeKitsuId && ac.CharacterId == character.Id);
                if (existingAnimeChar == null)
                {
                    context.AnimeCharacters.Add(new DbAnimeCharacter
                    {
                        AnimeKitsuId = animeKitsuId,
                        CharacterId = character.Id,
                        Role = character.Role
                    });
                }

                // Cache voice actors and relationships
                foreach (var voiceActor in character.VoiceActors)
                {
                    var existingVA = await context.VoiceActors.FirstOrDefaultAsync(va => va.Id == voiceActor.Id);
                    if (existingVA == null)
                    {
                        context.VoiceActors.Add(new DbVoiceActor
                        {
                            Id = voiceActor.Id,
                            NameRomaji = voiceActor.Name.Romaji,
                            NameFirst = voiceActor.Name.First,
                            NameLast = voiceActor.Name.Last,
                            NameFull = voiceActor.Name.Full,
                            NameNative = voiceActor.Name.Native,
                            NameAlternative = voiceActor.Name.Alternative,
                            NameAlternativeSpoiler = voiceActor.Name.AlternativeSpoiler
                        });
                    }

                    var existingCharVA = await context.CharacterVoiceActors
                        .FirstOrDefaultAsync(cva => cva.CharacterId == character.Id && cva.VoiceActorId == voiceActor.Id);
                    if (existingCharVA == null)
                    {
                        context.CharacterVoiceActors.Add(new DbCharacterVoiceActor
                        {
                            CharacterId = character.Id,
                            VoiceActorId = voiceActor.Id
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
}