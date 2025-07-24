using Microsoft.EntityFrameworkCore;
using AnimeCharacters.Data.Models;

namespace AnimeCharacters.Data
{
    public class AnimeCharactersDbContext : DbContext
    {
        public AnimeCharactersDbContext(DbContextOptions<AnimeCharactersDbContext> options) : base(options) { }

        public DbSet<DbUser> Users { get; set; }
        public DbSet<DbAnime> Anime { get; set; }
        public DbSet<DbLibraryEntry> LibraryEntries { get; set; }
        public DbSet<DbCharacter> Characters { get; set; }
        public DbSet<DbVoiceActor> VoiceActors { get; set; }
        public DbSet<DbCharacterVoiceActor> CharacterVoiceActors { get; set; }
        public DbSet<DbAnimeCharacter> AnimeCharacters { get; set; }
        public DbSet<DbSyncMetadata> SyncMetadata { get; set; }
        public DbSet<DbUserSettings> UserSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure indexes for performance
            modelBuilder.Entity<DbAnime>()
                .HasIndex(a => a.MyAnimeListId)
                .HasDatabaseName("IX_Anime_MyAnimeListId");

            modelBuilder.Entity<DbAnime>()
                .HasIndex(a => a.AnilistId)
                .HasDatabaseName("IX_Anime_AnilistId");

            modelBuilder.Entity<DbLibraryEntry>()
                .HasIndex(le => le.UserId)
                .HasDatabaseName("IX_LibraryEntry_UserId");

            modelBuilder.Entity<DbLibraryEntry>()
                .HasIndex(le => le.AnimeKitsuId)
                .HasDatabaseName("IX_LibraryEntry_AnimeKitsuId");

            modelBuilder.Entity<DbLibraryEntry>()
                .HasIndex(le => new { le.UserId, le.Status })
                .HasDatabaseName("IX_LibraryEntry_UserId_Status");

            modelBuilder.Entity<DbCharacterVoiceActor>()
                .HasIndex(cva => cva.CharacterId)
                .HasDatabaseName("IX_CharacterVoiceActor_CharacterId");

            modelBuilder.Entity<DbCharacterVoiceActor>()
                .HasIndex(cva => cva.VoiceActorId)
                .HasDatabaseName("IX_CharacterVoiceActor_VoiceActorId");

            modelBuilder.Entity<DbCharacterVoiceActor>()
                .HasIndex(cva => new { cva.CharacterId, cva.VoiceActorId })
                .IsUnique()
                .HasDatabaseName("IX_CharacterVoiceActor_Unique");

            modelBuilder.Entity<DbAnimeCharacter>()
                .HasIndex(ac => ac.AnimeKitsuId)
                .HasDatabaseName("IX_AnimeCharacter_AnimeKitsuId");

            modelBuilder.Entity<DbAnimeCharacter>()
                .HasIndex(ac => ac.CharacterId)
                .HasDatabaseName("IX_AnimeCharacter_CharacterId");

            modelBuilder.Entity<DbAnimeCharacter>()
                .HasIndex(ac => new { ac.AnimeKitsuId, ac.CharacterId })
                .IsUnique()
                .HasDatabaseName("IX_AnimeCharacter_Unique");

            modelBuilder.Entity<DbVoiceActor>()
                .HasIndex(va => va.Language)
                .HasDatabaseName("IX_VoiceActor_Language");

            // Configure relationships
            modelBuilder.Entity<DbLibraryEntry>()
                .HasOne(le => le.User)
                .WithMany(u => u.LibraryEntries)
                .HasForeignKey(le => le.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DbLibraryEntry>()
                .HasOne(le => le.Anime)
                .WithMany(a => a.LibraryEntries)
                .HasForeignKey(le => le.AnimeKitsuId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DbCharacterVoiceActor>()
                .HasOne(cva => cva.Character)
                .WithMany(c => c.CharacterVoiceActors)
                .HasForeignKey(cva => cva.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DbCharacterVoiceActor>()
                .HasOne(cva => cva.VoiceActor)
                .WithMany(va => va.CharacterVoiceActors)
                .HasForeignKey(cva => cva.VoiceActorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DbAnimeCharacter>()
                .HasOne(ac => ac.Anime)
                .WithMany(a => a.AnimeCharacters)
                .HasForeignKey(ac => ac.AnimeKitsuId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DbAnimeCharacter>()
                .HasOne(ac => ac.Character)
                .WithMany(c => c.AnimeCharacters)
                .HasForeignKey(ac => ac.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DbUserSettings>()
                .HasOne(us => us.User)
                .WithOne(u => u.Settings)
                .HasForeignKey<DbUserSettings>(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure enum conversions
            modelBuilder.Entity<DbLibraryEntry>()
                .Property(e => e.Type)
                .HasConversion<int>();

            modelBuilder.Entity<DbLibraryEntry>()
                .Property(e => e.Status)
                .HasConversion<int>();

            modelBuilder.Entity<DbAnime>()
                .Property(e => e.ShowType)
                .HasConversion<int>();

            modelBuilder.Entity<DbCharacter>()
                .Property(e => e.Role)
                .HasConversion<int>();

            modelBuilder.Entity<DbAnimeCharacter>()
                .Property(e => e.Role)
                .HasConversion<int>();

            modelBuilder.Entity<DbVoiceActor>()
                .Property(e => e.Language)
                .HasConversion<int>();

            modelBuilder.Entity<DbUserSettings>()
                .Property(e => e.PreferredTitleType)
                .HasConversion<int>();
        }
    }
}