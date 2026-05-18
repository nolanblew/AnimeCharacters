using AnimeCharacters.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitsu.Tests.AnimeCharacters
{
    [TestClass]
    public class CharactersPageTests
    {
        [TestMethod]
        public void Constructor_BeforeApiCompletes_StartsInLoadingState()
        {
            var page = new Characters();

            Assert.IsTrue(page.IsLoadingCharacters);
        }
    }
}
