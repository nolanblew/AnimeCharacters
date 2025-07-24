using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kitsu.Controllers;
using Kitsu.Models;

namespace AnimeCharacters.Data.Models
{
    public class DbLibraryEntry
    {
        [Key]
        public long Id { get; set; }
        
        public LibraryType Type { get; set; }
        public LibraryStatus Status { get; set; }
        
        public long? MangaId { get; set; }
        public bool IsReconsuming { get; set; }
        
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public DateTimeOffset? ProgressedAt { get; set; }
        
        public long? Progress { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [MaxLength(50)]
        public string AnimeKitsuId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [ForeignKey(nameof(UserId))]
        public DbUser User { get; set; }
        
        [ForeignKey(nameof(AnimeKitsuId))]
        public DbAnime Anime { get; set; }
    }
}