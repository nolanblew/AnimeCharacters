using AnimeCharacters.Data.Extensions;
using Kitsu.Models;

namespace AnimeCharacters.Tests
{
    [TestClass]
    public class ModelConversionExtensionsTests
    {
        [TestMethod]
        public void ToDbUser_WithValidUser_ShouldConvertCorrectly()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Name = "Test User",
                Username = "testuser",
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            // Act
            var dbUser = user.ToDbUser();

            // Assert
            Assert.AreEqual(1, dbUser.Id);
            Assert.AreEqual("Test User", dbUser.Name);
            Assert.AreEqual("testuser", dbUser.Username);
            Assert.AreEqual("https://example.com/avatar.jpg", dbUser.AvatarUrl);
        }

        [TestMethod]
        public void ToDbUser_WithNullUsername_ShouldUseDefault()
        {
            // Arrange
            var user = new User
            {
                Id = 2,
                Name = "Test User",
                Username = null,
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            // Act
            var dbUser = user.ToDbUser();

            // Assert
            Assert.AreEqual("user_2", dbUser.Username);
        }

        [TestMethod]
        public void ToDbUser_WithNullName_ShouldUseDefault()
        {
            // Arrange
            var user = new User
            {
                Id = 3,
                Name = null,
                Username = "testuser",
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            // Act
            var dbUser = user.ToDbUser();

            // Assert
            Assert.AreEqual("Unknown User", dbUser.Name);
        }

        [TestMethod]
        public void ToUser_WithDbUser_ShouldConvertCorrectly()
        {
            // Arrange
            var dbUser = new AnimeCharacters.Data.Models.DbUser
            {
                Id = 1,
                Name = "Test User",
                Username = "testuser",
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            // Act
            var user = dbUser.ToUser();

            // Assert
            Assert.AreEqual(1, user.Id);
            Assert.AreEqual("Test User", user.Name);
            Assert.AreEqual("testuser", user.Username);
            Assert.AreEqual("https://example.com/avatar.jpg", user.AvatarUrl);
        }

        [TestMethod]
        public void ToDbAnime_WithValidAnime_ShouldConvertCorrectly()
        {
            // Arrange
            var anime = new Kitsu.Models.Anime(
                "123",
                456,
                789,
                "Test Anime",
                "Test Anime Romanji",
                "Test Anime English",
                "test-anime",
                "https://example.com/poster.jpg",
                "abc123",
                "TV"
            );

            // Act
            var dbAnime = anime.ToDbAnime();

            // Assert
            Assert.AreEqual("123", dbAnime.KitsuId);
            Assert.AreEqual((long?)456, dbAnime.MyAnimeListId);
            Assert.AreEqual((long?)789, dbAnime.AnilistId);
            Assert.AreEqual("Test Anime", dbAnime.Title);
            Assert.AreEqual("Test Anime Romanji", dbAnime.RomanjiTitle);
            Assert.AreEqual("Test Anime English", dbAnime.EnglishTitle);
            Assert.AreEqual("test-anime", dbAnime.KitsuSlug);
            Assert.AreEqual("https://example.com/poster.jpg", dbAnime.PosterImageUrl);
            Assert.AreEqual("abc123", dbAnime.YoutubeId);
            Assert.AreEqual("TV", dbAnime.ShowType);
        }

        [TestMethod]
        public void ToAnime_WithDbAnime_ShouldConvertCorrectly()
        {
            // Arrange
            var dbAnime = new AnimeCharacters.Data.Models.DbAnime
            {
                KitsuId = "123",
                MyAnimeListId = 456,
                AnilistId = 789,
                Title = "Test Anime",
                RomanjiTitle = "Test Anime Romanji",
                EnglishTitle = "Test Anime English",
                KitsuSlug = "test-anime",
                PosterImageUrl = "https://example.com/poster.jpg",
                YoutubeId = "abc123",
                ShowType = "TV"
            };

            // Act
            var anime = dbAnime.ToAnime();

            // Assert
            Assert.AreEqual("123", anime.KitsuId);
            Assert.AreEqual((long?)456, anime.MyAnimeListId);
            Assert.AreEqual((long?)789, anime.AnilistId);
            Assert.AreEqual("Test Anime", anime.Title);
            Assert.AreEqual("Test Anime Romanji", anime.RomanjiTitle);
            Assert.AreEqual("Test Anime English", anime.EnglishTitle);
            Assert.AreEqual("test-anime", anime.KitsuSlug);
            Assert.AreEqual("https://example.com/poster.jpg", anime.PosterImageUrl);
            Assert.AreEqual("abc123", anime.YoutubeId);
            Assert.AreEqual("TV", anime.ShowType);
        }
    }
}