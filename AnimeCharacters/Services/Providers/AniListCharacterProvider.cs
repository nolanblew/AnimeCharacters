using AnimeCharacters.Models;
using AnimeCharacters.Services;
using AniListClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeCharacters.Services.Providers
{
    /// <summary>
    /// AniList provider for character and voice actor data
    /// </summary>
    public class AniListCharacterProvider : ICharacterDataProvider
    {
        private readonly AniListClient.AniListClient _anilistClient;

        public AniListCharacterProvider(AniListClient.AniListClient anilistClient)
        {
            _anilistClient = anilistClient;
        }

        public string ProviderId => "anilist";
        public string ProviderName => "AniList";
        public int Priority => 90; // High priority for character data
        public bool IsEnabled => true;

        public async Task<List<UnifiedCharacter>> GetCharactersForAnimeAsync(string animeId)
        {
            var media = await _anilistClient.Characters.GetMediaWithCharactersById(int.Parse(animeId));
            return media.Characters.Select(ConvertToUnifiedCharacter).ToList();
        }

        public async Task<UnifiedVoiceActor> GetVoiceActorAsync(string voiceActorId)
        {
            var staff = await _anilistClient.Staff.GetStaffById(int.Parse(voiceActorId));
            return ConvertToUnifiedVoiceActor(staff);
        }

        public async Task<List<UnifiedCharacter>> GetCharactersByVoiceActorAsync(string voiceActorId)
        {
            var staff = await _anilistClient.Staff.GetStaffById(int.Parse(voiceActorId));
            if (staff?.Characters == null)
                return new List<UnifiedCharacter>();

            return staff.Characters.Select(ConvertToUnifiedCharacter).ToList();
        }

        private UnifiedCharacter ConvertToUnifiedCharacter(AniListClient.Models.Character anilistCharacter)
        {
            var unified = new UnifiedCharacter
            {
                Id = anilistCharacter.Id.ToString(),
                Name = anilistCharacter.Name?.Full,
                RomanjiName = anilistCharacter.Name?.Romaji,
                EnglishName = anilistCharacter.Name?.First + " " + anilistCharacter.Name?.Last,
                ImageUrl = anilistCharacter.Image?.Large,
                Description = anilistCharacter.Description,
                Role = ConvertCharacterRole(anilistCharacter.Role),
                VoiceActors = anilistCharacter.VoiceActors?.Select(ConvertToUnifiedVoiceActorSlim).ToList() ?? new List<UnifiedVoiceActor>(),
                ProviderId = ProviderId,
                OriginalData = anilistCharacter
            };

            unified.ProviderIds[ProviderId] = anilistCharacter.Id.ToString();
            return unified;
        }

        private UnifiedVoiceActor ConvertToUnifiedVoiceActor(AniListClient.Models.Staff anilistStaff)
        {
            if (anilistStaff == null) return null;

            var unified = new UnifiedVoiceActor
            {
                Id = anilistStaff.Id.ToString(),
                Name = anilistStaff.Name?.Full,
                RomanjiName = anilistStaff.Name?.Romaji,
                EnglishName = anilistStaff.Name?.First + " " + anilistStaff.Name?.Last,
                ImageUrl = anilistStaff.Images?.Large,
                Description = anilistStaff.Description,
                Age = anilistStaff.Age,
                DateOfBirth = ConvertDateOfBirth(anilistStaff.DateOfBirth),
                BloodType = anilistStaff.BloodType,
                Language = ConvertLanguage(anilistStaff.Language),
                Characters = anilistStaff.Characters?.Select(ConvertToUnifiedCharacter).ToList() ?? new List<UnifiedCharacter>(),
                ProviderId = ProviderId,
                OriginalData = anilistStaff
            };

            unified.ProviderIds[ProviderId] = anilistStaff.Id.ToString();
            return unified;
        }

        private UnifiedVoiceActor ConvertToUnifiedVoiceActorSlim(AniListClient.Models.VoiceActorSlim anilistVA)
        {
            if (anilistVA == null) return null;

            var unified = new UnifiedVoiceActor
            {
                Id = anilistVA.Id.ToString(),
                Name = anilistVA.Name?.Full,
                RomanjiName = anilistVA.Name?.Romaji,
                EnglishName = anilistVA.Name?.First + " " + anilistVA.Name?.Last,
                // ImageUrl and Language not available in VoiceActorSlim
                ProviderId = ProviderId,
                OriginalData = anilistVA
            };

            unified.ProviderIds[ProviderId] = anilistVA.Id.ToString();
            return unified;
        }

        private Models.CharacterRole? ConvertCharacterRole(AniListClient.Models.CharacterRole? anilistRole)
        {
            return anilistRole switch
            {
                AniListClient.Models.CharacterRole.Main => Models.CharacterRole.Main,
                AniListClient.Models.CharacterRole.Supporting => Models.CharacterRole.Supporting,
                AniListClient.Models.CharacterRole.Background => Models.CharacterRole.Background,
                _ => null
            };
        }

        private Models.VoiceActorLanguage? ConvertLanguage(AniListClient.Models.Language? anilistLanguage)
        {
            return anilistLanguage switch
            {
                AniListClient.Models.Language.Japanese => Models.VoiceActorLanguage.Japanese,
                AniListClient.Models.Language.English => Models.VoiceActorLanguage.English,
                AniListClient.Models.Language.Korean => Models.VoiceActorLanguage.Korean,
                AniListClient.Models.Language.Italian => Models.VoiceActorLanguage.Italian,
                AniListClient.Models.Language.Spanish => Models.VoiceActorLanguage.Spanish,
                AniListClient.Models.Language.Portuguese => Models.VoiceActorLanguage.Portuguese,
                AniListClient.Models.Language.French => Models.VoiceActorLanguage.French,
                AniListClient.Models.Language.German => Models.VoiceActorLanguage.German,
                AniListClient.Models.Language.Hebrew => Models.VoiceActorLanguage.Hebrew,
                AniListClient.Models.Language.Hungarian => Models.VoiceActorLanguage.Hungarian,
                AniListClient.Models.Language.Chinese => Models.VoiceActorLanguage.Chinese,
                AniListClient.Models.Language.Arabic => Models.VoiceActorLanguage.Arabic,
                AniListClient.Models.Language.Filipino => Models.VoiceActorLanguage.Filipino,
                AniListClient.Models.Language.Catalan => Models.VoiceActorLanguage.Catalan,
                _ => null
            };
        }

        private UnifiedDateOfBirth ConvertDateOfBirth(AniListClient.Models.DateOfBirth anilistDOB)
        {
            if (anilistDOB == null) return null;

            return new UnifiedDateOfBirth
            {
                Year = anilistDOB.Year,
                Month = anilistDOB.Month,
                Day = anilistDOB.Day
            };
        }
    }
}