using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimeCharacters.Data.Models
{
    public class DbCharacterVoiceActor
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int CharacterId { get; set; }
        
        [Required]
        public int VoiceActorId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [ForeignKey(nameof(CharacterId))]
        public DbCharacter Character { get; set; }
        
        [ForeignKey(nameof(VoiceActorId))]
        public DbVoiceActor VoiceActor { get; set; }
    }
}