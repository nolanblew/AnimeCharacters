using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AniListClient.Models;

namespace AnimeCharacters.Data.Models
{
    public class DbAnimeCharacter
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string AnimeKitsuId { get; set; }
        
        [Required]
        public int CharacterId { get; set; }
        
        public CharacterRole? Role { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [ForeignKey(nameof(AnimeKitsuId))]
        public DbAnime Anime { get; set; }
        
        [ForeignKey(nameof(CharacterId))]
        public DbCharacter Character { get; set; }
    }
}