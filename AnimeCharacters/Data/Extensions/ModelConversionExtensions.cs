using AnimeCharacters.Data.Models;
using AnimeCharacters.Models;
using AniListClient.Models;
using Kitsu.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnimeCharacters.Data.Extensions
{
    public static class ModelConversionExtensions
    {
        // User conversions
        public static DbUser ToDbUser(this User user)
        {
            return new DbUser
            {
                Id = user.Id,
                Name = user.Name,
                Username = user.Username,
                AvatarUrl = user.AvatarUrl,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static User ToUser(this DbUser dbUser)
        {
            return new User
            {
                Id = dbUser.Id,
                Name = dbUser.Name,
                Username = dbUser.Username,
                AvatarUrl = dbUser.AvatarUrl
            };
        }

        // Anime conversions
        public static DbAnime ToDbAnime(this Anime anime)
        {
            return new DbAnime
            {
                KitsuId = anime.KitsuId,
                MyAnimeListId = anime.MyAnimeListId,
                AnilistId = anime.AnilistId,
                Title = anime.Title,
                RomanjiTitle = anime.RomanjiTitle,
                EnglishTitle = anime.EnglishTitle,
                PosterImageUrl = anime.PosterImageUrl,
                KitsuSlug = anime.KitsuSlug,
                YoutubeId = anime.YoutubeId,
                ShowType = anime.ShowType,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Anime ToAnime(this DbAnime dbAnime)
        {
            return new Anime(
                dbAnime.KitsuId,
                dbAnime.MyAnimeListId,
                dbAnime.AnilistId,
                dbAnime.Title,
                dbAnime.RomanjiTitle,
                dbAnime.EnglishTitle,
                dbAnime.KitsuSlug,
                dbAnime.PosterImageUrl,
                dbAnime.YoutubeId,
                dbAnime.ShowType);
        }

        // LibraryEntry conversions
        public static DbLibraryEntry ToDbLibraryEntry(this LibraryEntry libraryEntry)
        {
            return new DbLibraryEntry
            {
                Id = libraryEntry.Id,
                Type = libraryEntry.Type,
                Status = libraryEntry.Status,
                AnimeKitsuId = libraryEntry.AnimeId?.ToString(),
                MangaId = libraryEntry.MangaId,
                IsReconsuming = libraryEntry.IsReconsuming,
                StartedAt = libraryEntry.StartedAt,
                FinishedAt = libraryEntry.FinishedAt,
                ProgressedAt = libraryEntry.ProgressedAt,
                Progress = libraryEntry.Progress,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static LibraryEntry ToLibraryEntry(this DbLibraryEntry dbLibraryEntry)
        {
            var libraryEntry = new LibraryEntry
            {
                Id = dbLibraryEntry.Id,
                Type = dbLibraryEntry.Type,
                Status = dbLibraryEntry.Status,
                AnimeId = long.TryParse(dbLibraryEntry.AnimeKitsuId, out var animeId) ? animeId : null,
                MangaId = dbLibraryEntry.MangaId,
                IsReconsuming = dbLibraryEntry.IsReconsuming,
                StartedAt = dbLibraryEntry.StartedAt,
                FinishedAt = dbLibraryEntry.FinishedAt,
                ProgressedAt = dbLibraryEntry.ProgressedAt,
                Progress = dbLibraryEntry.Progress
            };

            if (dbLibraryEntry.Anime != null)
            {
                libraryEntry.Anime = dbLibraryEntry.Anime.ToAnime();
            }

            return libraryEntry;
        }

        // Character conversions
        public static DbCharacter ToDbCharacter(this Character character)
        {
            return new DbCharacter
            {
                Id = character.Id,
                NameRomaji = character.Name.Romaji,
                NameFirst = character.Name.First,
                NameLast = character.Name.Last,
                NameFull = character.Name.Full,
                NameNative = character.Name.Native,
                NameAlternative = character.Name.Alternative,
                NameAlternativeSpoiler = character.Name.AlternativeSpoiler,
                ImageMedium = character.Image.Medium,
                ImageLarge = character.Image.Large,
                ImageExtraLarge = character.Image.ExtraLarge,
                ImageColor = character.Image.Color,
                Description = character.Description,
                Role = character.Role,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Character ToCharacter(this DbCharacter dbCharacter)
        {
            var names = new Names(
                dbCharacter.NameRomaji,
                dbCharacter.NameFirst,
                dbCharacter.NameLast,
                dbCharacter.NameFull,
                dbCharacter.NameNative,
                dbCharacter.NameAlternative,
                dbCharacter.NameAlternativeSpoiler);

            var images = new Images(
                dbCharacter.ImageMedium,
                dbCharacter.ImageLarge,
                dbCharacter.ImageExtraLarge,
                dbCharacter.ImageColor);

            var voiceActors = dbCharacter.CharacterVoiceActors
                ?.Select(cva => new VoiceActorSlim(cva.VoiceActor.Id, new Names(
                    cva.VoiceActor.NameRomaji,
                    cva.VoiceActor.NameFirst,
                    cva.VoiceActor.NameLast,
                    cva.VoiceActor.NameFull,
                    cva.VoiceActor.NameNative,
                    cva.VoiceActor.NameAlternative,
                    cva.VoiceActor.NameAlternativeSpoiler)))
                .ToList() ?? new List<VoiceActorSlim>();

            return new Character(
                dbCharacter.Id,
                names,
                images,
                dbCharacter.Description,
                dbCharacter.Role,
                new List<MediaBase>(), // Media will be populated separately if needed
                voiceActors);
        }

        // VoiceActor conversions
        public static DbVoiceActor ToDbVoiceActor(this Staff staff)
        {
            return new DbVoiceActor
            {
                Id = staff.Id,
                NameRomaji = staff.Name.Romaji,
                NameFirst = staff.Name.First,
                NameLast = staff.Name.Last,
                NameFull = staff.Name.Full,
                NameNative = staff.Name.Native,
                NameAlternative = staff.Name.Alternative,
                NameAlternativeSpoiler = staff.Name.AlternativeSpoiler,
                Language = staff.Language,
                ImageMedium = staff.Images.Medium,
                ImageLarge = staff.Images.Large,
                ImageExtraLarge = staff.Images.ExtraLarge,
                ImageColor = staff.Images.Color,
                Description = staff.Description,
                Age = staff.Age,
                BirthYear = staff.DateOfBirth.Year,
                BirthMonth = staff.DateOfBirth.Month,
                BirthDay = staff.DateOfBirth.Day,
                BloodType = staff.BloodType,
                SiteUrl = staff.SiteUrl,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Staff ToStaff(this DbVoiceActor dbVoiceActor)
        {
            var names = new Names(
                dbVoiceActor.NameRomaji,
                dbVoiceActor.NameFirst,
                dbVoiceActor.NameLast,
                dbVoiceActor.NameFull,
                dbVoiceActor.NameNative,
                dbVoiceActor.NameAlternative,
                dbVoiceActor.NameAlternativeSpoiler);

            var images = new Images(
                dbVoiceActor.ImageMedium,
                dbVoiceActor.ImageLarge,
                dbVoiceActor.ImageExtraLarge,
                dbVoiceActor.ImageColor);

            var dateOfBirth = new DateOfBirth(
                dbVoiceActor.BirthYear,
                dbVoiceActor.BirthMonth,
                dbVoiceActor.BirthDay);

            return new Staff(
                dbVoiceActor.Id,
                names,
                dbVoiceActor.Language,
                images,
                dbVoiceActor.Description,
                dbVoiceActor.Age,
                dateOfBirth,
                dbVoiceActor.BloodType,
                dbVoiceActor.SiteUrl,
                new List<Character>()); // Characters will be populated separately if needed
        }

        // UserSettings conversions
        public static DbUserSettings ToDbUserSettings(this UserSettings settings, int userId)
        {
            return new DbUserSettings
            {
                UserId = userId,
                PreferredTitleType = settings.PreferredTitleType,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static UserSettings ToUserSettings(this DbUserSettings dbSettings)
        {
            return new UserSettings
            {
                PreferredTitleType = dbSettings.PreferredTitleType
            };
        }
    }
}