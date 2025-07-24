using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AnimeCharacters.Data.Models
{
    public class DbUser
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Username { get; set; }
        
        [MaxLength(500)]
        public string AvatarUrl { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<DbLibraryEntry> LibraryEntries { get; set; } = new List<DbLibraryEntry>();
        public DbUserSettings Settings { get; set; }
    }
}