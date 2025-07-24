using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AnimeCharacters.Models;

namespace AnimeCharacters.Data.Models
{
    public class DbUserSettings
    {
        [Key, ForeignKey(nameof(User))]
        public int UserId { get; set; }
        
        public TitleType PreferredTitleType { get; set; } = TitleType.UserPreferred;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public DbUser User { get; set; }
    }
}