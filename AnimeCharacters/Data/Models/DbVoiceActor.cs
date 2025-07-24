using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AniListClient.Models;

namespace AnimeCharacters.Data.Models
{
    public class DbVoiceActor
    {
        [Key]
        public int Id { get; set; }
        
        [MaxLength(255)]
        public string NameRomaji { get; set; }
        
        [MaxLength(255)]
        public string NameFirst { get; set; }
        
        [MaxLength(255)]
        public string NameLast { get; set; }
        
        [MaxLength(255)]
        public string NameFull { get; set; }
        
        [MaxLength(255)]
        public string NameNative { get; set; }
        
        [MaxLength(255)]
        public string NameAlternative { get; set; }
        
        [MaxLength(255)]
        public string NameAlternativeSpoiler { get; set; }
        
        public Language Language { get; set; }
        
        [MaxLength(1000)]
        public string ImageMedium { get; set; }
        
        [MaxLength(1000)]
        public string ImageLarge { get; set; }
        
        [MaxLength(1000)]
        public string ImageExtraLarge { get; set; }
        
        [MaxLength(50)]
        public string ImageColor { get; set; }
        
        public string Description { get; set; }
        
        public int? Age { get; set; }
        
        public int? BirthYear { get; set; }
        public int? BirthMonth { get; set; }
        public int? BirthDay { get; set; }
        
        [MaxLength(10)]
        public string BloodType { get; set; }
        
        [MaxLength(500)]
        public string SiteUrl { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<DbCharacterVoiceActor> CharacterVoiceActors { get; set; } = new List<DbCharacterVoiceActor>();
    }
}