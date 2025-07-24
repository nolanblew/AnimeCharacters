using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kitsu.Models;

namespace AnimeCharacters.Data.Models
{
    public class DbAnime
    {
        [Key]
        [MaxLength(50)]
        public string KitsuId { get; set; }
        
        [MaxLength(50)]
        public string MyAnimeListId { get; set; }
        
        [MaxLength(50)]
        public string AnilistId { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Title { get; set; }
        
        [MaxLength(500)]
        public string RomanjiTitle { get; set; }
        
        [MaxLength(500)]
        public string EnglishTitle { get; set; }
        
        [MaxLength(1000)]
        public string PosterImageUrl { get; set; }
        
        [MaxLength(255)]
        public string KitsuSlug { get; set; }
        
        [MaxLength(255)]
        public string YoutubeId { get; set; }
        
        public AnimeType? ShowType { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<DbLibraryEntry> LibraryEntries { get; set; } = new List<DbLibraryEntry>();
        public ICollection<DbAnimeCharacter> AnimeCharacters { get; set; } = new List<DbAnimeCharacter>();
    }
}