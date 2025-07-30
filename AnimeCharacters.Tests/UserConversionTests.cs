using AnimeCharacters.Data.Extensions;
using Kitsu.Models;

namespace AnimeCharacters.Tests
{
    [TestClass]
    public class UserConversionTests
    {
        [TestMethod]
        public void ToDbUser_WithNullUsername_ShouldUseDefaultUsername()
        {
            // Arrange
            var user = new User
            {
                Id = 123,
                Name = "Test User",
                Username = null, // This was causing the original database error
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            // Act
            var dbUser = user.ToDbUser();

            // Assert
            Assert.AreEqual(123, dbUser.Id);
            Assert.AreEqual("Test User", dbUser.Name);
            Assert.AreEqual("user_123", dbUser.Username); // Should use default format
            Assert.AreEqual("https://example.com/avatar.jpg", dbUser.AvatarUrl);
            Assert.IsTrue(dbUser.UpdatedAt <= DateTime.UtcNow);
        }

        [TestMethod]
        public void ToDbUser_WithNullName_ShouldUseDefaultName()
        {
            // Arrange
            var user = new User
            {
                Id = 456,
                Name = null, // This could also cause database issues
                Username = "testuser",
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            // Act
            var dbUser = user.ToDbUser();

            // Assert
            Assert.AreEqual(456, dbUser.Id);
            Assert.AreEqual("Unknown User", dbUser.Name); // Should use default name
            Assert.AreEqual("testuser", dbUser.Username);
            Assert.AreEqual("https://example.com/avatar.jpg", dbUser.AvatarUrl);
            Assert.IsTrue(dbUser.UpdatedAt <= DateTime.UtcNow);
        }

        [TestMethod]
        public void ToDbUser_WithValidData_ShouldPreserveData()
        {
            // Arrange
            var user = new User
            {
                Id = 789,
                Name = "Valid User",
                Username = "validuser",
                AvatarUrl = "https://example.com/valid.jpg"
            };

            // Act
            var dbUser = user.ToDbUser();

            // Assert
            Assert.AreEqual(789, dbUser.Id);
            Assert.AreEqual("Valid User", dbUser.Name);
            Assert.AreEqual("validuser", dbUser.Username);
            Assert.AreEqual("https://example.com/valid.jpg", dbUser.AvatarUrl);
            Assert.IsTrue(dbUser.UpdatedAt <= DateTime.UtcNow);
        }

        [TestMethod]
        public void ToUser_FromDbUser_ShouldConvertCorrectly()
        {
            // Arrange
            var dbUser = new AnimeCharacters.Data.Models.DbUser
            {
                Id = 101,
                Name = "Database User",
                Username = "dbuser",
                AvatarUrl = "https://example.com/db.jpg",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            var user = dbUser.ToUser();

            // Assert
            Assert.AreEqual(101, user.Id);
            Assert.AreEqual("Database User", user.Name);
            Assert.AreEqual("dbuser", user.Username);
            Assert.AreEqual("https://example.com/db.jpg", user.AvatarUrl);
        }
    }
}