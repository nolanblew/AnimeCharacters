using System;
using System.Collections.Generic;

namespace AnimeCharacters.Models
{
    /// <summary>
    /// Unified user model that can represent users from different providers
    /// </summary>
    public class UnifiedUser
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
        public string ProviderId { get; set; }
        public Dictionary<string, string> ProviderSpecificIds { get; set; } = new();
        public object OriginalData { get; set; }
    }

    /// <summary>
    /// Unified anime model that can represent anime from different sources
    /// </summary>
    public class UnifiedAnime
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string RomanjiTitle { get; set; }
        public string EnglishTitle { get; set; }
        public string PosterImageUrl { get; set; }
        public string Description { get; set; }
        public AnimeType? Type { get; set; }
        public string ProviderId { get; set; }
        
        /// <summary>
        /// Cross-provider identifiers (e.g., "kitsu" -> "123", "anilist" -> "456")
        /// </summary>
        public Dictionary<string, string> ProviderIds { get; set; } = new();
        
        public object OriginalData { get; set; }
    }

    /// <summary>
    /// Unified library entry that represents a user's anime in their library
    /// </summary>
    public class UnifiedLibraryEntry
    {
        public string Id { get; set; }
        public UnifiedAnime Anime { get; set; }
        public LibraryStatus Status { get; set; }
        public int Progress { get; set; }
        public DateTimeOffset? ProgressedAt { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public bool IsReconsuming { get; set; }
        public string ProviderId { get; set; }
        public object OriginalData { get; set; }
    }

    /// <summary>
    /// Unified character model
    /// </summary>
    public class UnifiedCharacter
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RomanjiName { get; set; }
        public string EnglishName { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public CharacterRole? Role { get; set; }
        public List<UnifiedVoiceActor> VoiceActors { get; set; } = new();
        public List<UnifiedAnime> AppearancesIn { get; set; } = new();
        public string ProviderId { get; set; }
        public Dictionary<string, string> ProviderIds { get; set; } = new();
        public object OriginalData { get; set; }
    }

    /// <summary>
    /// Unified voice actor model
    /// </summary>
    public class UnifiedVoiceActor
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RomanjiName { get; set; }
        public string EnglishName { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public int? Age { get; set; }
        public UnifiedDateOfBirth DateOfBirth { get; set; }
        public string BloodType { get; set; }
        public VoiceActorLanguage? Language { get; set; }
        public List<UnifiedCharacter> Characters { get; set; } = new();
        public string ProviderId { get; set; }
        public Dictionary<string, string> ProviderIds { get; set; } = new();
        public object OriginalData { get; set; }
    }

    /// <summary>
    /// Unified date of birth
    /// </summary>
    public class UnifiedDateOfBirth
    {
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }
    }

    /// <summary>
    /// Enums for unified data
    /// </summary>
    public enum CharacterRole
    {
        Main,
        Supporting,
        Background
    }

    public enum VoiceActorLanguage
    {
        Japanese,
        English,
        Korean,
        Italian,
        Spanish,
        Portuguese,
        French,
        German,
        Hebrew,
        Hungarian,
        Chinese,
        Arabic,
        Filipino,
        Catalan
    }

    // Reuse existing enums from Kitsu
    public enum LibraryStatus
    {
        Current = 1,
        Completed = 2,
        OnHold = 3,
        Dropped = 4,
        Planned = 5
    }

    public enum AnimeType
    {
        Show = 0,
        Movie = 1,
        OVA = 2
    }
}