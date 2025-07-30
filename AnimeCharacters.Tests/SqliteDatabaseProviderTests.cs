using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using AnimeCharacters.Data;
using AnimeCharacters.Data.Extensions;
using AnimeCharacters.Models;
using Kitsu.Models;
using EventAggregator.Blazor;
using Moq;

namespace AnimeCharacters.Tests
{
    [TestClass]
    public class SqliteDatabaseProviderTests
    {
        private DbContextOptions<AnimeCharactersDbContext> GetInMemoryDbOptions()
        {
            return new DbContextOptionsBuilder<AnimeCharactersDbContext>()
                .UseInMemory(Guid.NewGuid().ToString())
                .Options;
        }

        private Mock<IEventAggregator> CreateMockEventAggregator()
        {
            return new Mock<IEventAggregator>();
        }

        [TestMethod]
        public async Task SetUserAsync_WithValidUser_ShouldSaveSuccessfully()
        {
            // Arrange
            var options = GetInMemoryDbOptions();
            var contextFactory = new TestDbContextFactory(options);
            var mockEventAggregator = CreateMockEventAggregator();
            var provider = new SqliteDatabaseProvider(contextFactory, mockEventAggregator.Object);

            var user = new User
            {
                Id = 1,
                Name = "Test User",
                Username = "testuser",
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            // Act
            await provider.SetUserAsync(user);

            // Assert
            using var context = new AnimeCharactersDbContext(options);
            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == 1);
            
            Assert.IsNotNull(savedUser);
            Assert.AreEqual("Test User", savedUser.Name);
            Assert.AreEqual("testuser", savedUser.Username);
            Assert.AreEqual("https://example.com/avatar.jpg", savedUser.AvatarUrl);
        }

        [TestMethod]
        public async Task SetUserAsync_WithNullUsername_ShouldUseDefaultUsername()
        {
            // Arrange
            var options = GetInMemoryDbOptions();
            var contextFactory = new TestDbContextFactory(options);
            var mockEventAggregator = CreateMockEventAggregator();
            var provider = new SqliteDatabaseProvider(contextFactory, mockEventAggregator.Object);

            var user = new User
            {
                Id = 2,
                Name = "Test User",
                Username = null, // This causes the original error
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            // Act
            await provider.SetUserAsync(user);

            // Assert
            using var context = new AnimeCharactersDbContext(options);
            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == 2);
            
            Assert.IsNotNull(savedUser);
            Assert.AreEqual("Test User", savedUser.Name);
            Assert.AreEqual("user_2", savedUser.Username); // Should use default format
            Assert.AreEqual("https://example.com/avatar.jpg", savedUser.AvatarUrl);
        }

        [TestMethod]
        public async Task SetUserAsync_WithNullName_ShouldUseDefaultName()
        {
            // Arrange
            var options = GetInMemoryDbOptions();
            var contextFactory = new TestDbContextFactory(options);
            var mockEventAggregator = CreateMockEventAggregator();
            var provider = new SqliteDatabaseProvider(contextFactory, mockEventAggregator.Object);

            var user = new User
            {
                Id = 3,
                Name = null, // This could also cause issues
                Username = "testuser",
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            // Act
            await provider.SetUserAsync(user);

            // Assert
            using var context = new AnimeCharactersDbContext(options);
            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == 3);
            
            Assert.IsNotNull(savedUser);
            Assert.AreEqual("Unknown User", savedUser.Name); // Should use default name
            Assert.AreEqual("testuser", savedUser.Username);
            Assert.AreEqual("https://example.com/avatar.jpg", savedUser.AvatarUrl);
        }

        [TestMethod]
        public async Task SetUserAsync_UpdateExistingUser_ShouldUpdateSuccessfully()
        {
            // Arrange
            var options = GetInMemoryDbOptions();
            var contextFactory = new TestDbContextFactory(options);
            var mockEventAggregator = CreateMockEventAggregator();
            var provider = new SqliteDatabaseProvider(contextFactory, mockEventAggregator.Object);

            var originalUser = new User
            {
                Id = 4,
                Name = "Original Name",
                Username = "original",
                AvatarUrl = "https://example.com/old.jpg"
            };

            var updatedUser = new User
            {
                Id = 4,
                Name = "Updated Name",
                Username = "updated",
                AvatarUrl = "https://example.com/new.jpg"
            };

            // Act
            await provider.SetUserAsync(originalUser);
            await provider.SetUserAsync(updatedUser);

            // Assert
            using var context = new AnimeCharactersDbContext(options);
            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == 4);
            
            Assert.IsNotNull(savedUser);
            Assert.AreEqual("Updated Name", savedUser.Name);
            Assert.AreEqual("updated", savedUser.Username);
            Assert.AreEqual("https://example.com/new.jpg", savedUser.AvatarUrl);
        }

        [TestMethod]
        public async Task GetUserAsync_WithExistingUser_ShouldReturnUser()
        {
            // Arrange
            var options = GetInMemoryDbOptions();
            var contextFactory = new TestDbContextFactory(options);
            var mockEventAggregator = CreateMockEventAggregator();
            var provider = new SqliteDatabaseProvider(contextFactory, mockEventAggregator.Object);

            var user = new User
            {
                Id = 5,
                Name = "Test User",
                Username = "testuser",
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            await provider.SetUserAsync(user);

            // Act
            var retrievedUser = await provider.GetUserAsync();

            // Assert
            Assert.IsNotNull(retrievedUser);
            Assert.AreEqual(5, retrievedUser.Id);
            Assert.AreEqual("Test User", retrievedUser.Name);
            Assert.AreEqual("testuser", retrievedUser.Username);
            Assert.AreEqual("https://example.com/avatar.jpg", retrievedUser.AvatarUrl);
        }

        [TestMethod]
        public async Task GetUserAsync_WithNoUser_ShouldReturnNull()
        {
            // Arrange
            var options = GetInMemoryDbOptions();
            var contextFactory = new TestDbContextFactory(options);
            var mockEventAggregator = CreateMockEventAggregator();
            var provider = new SqliteDatabaseProvider(contextFactory, mockEventAggregator.Object);

            // Act
            var retrievedUser = await provider.GetUserAsync();

            // Assert
            Assert.IsNull(retrievedUser);
        }
    }

    // Helper class for testing
    public class TestDbContextFactory : IDbContextFactory<AnimeCharactersDbContext>
    {
        private readonly DbContextOptions<AnimeCharactersDbContext> _options;

        public TestDbContextFactory(DbContextOptions<AnimeCharactersDbContext> options)
        {
            _options = options;
        }

        public AnimeCharactersDbContext CreateDbContext()
        {
            return new AnimeCharactersDbContext(_options);
        }

        public async Task<AnimeCharactersDbContext> CreateDbContextAsync()
        {
            return await Task.FromResult(new AnimeCharactersDbContext(_options));
        }
    }
}