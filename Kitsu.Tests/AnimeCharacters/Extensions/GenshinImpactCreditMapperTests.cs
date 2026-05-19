using AnimeCharacters.Extensions;
using AnimeCharacters.Extensions.GenshinImpact;
using AniListClient.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Kitsu.Tests.AnimeCharacters.Extensions
{
    [TestClass]
    public class GenshinImpactCreditMapperTests
    {
        [TestMethod]
        public void CreateCredits_WhenStaffNameMatchesEasternOrder_ReturnsGenshinCredit()
        {
            var staff = CreateStaff(fullName: "Tomoaki Maeno", nativeName: "前野智昭");
            var characters = new List<GenshinImpactCharacter>
            {
                new()
                {
                    Name = "Zhongli",
                    ImageUrl = "https://example.test/zhongli.png",
                    WikiUrl = "https://genshin-impact.fandom.com/wiki/Zhongli",
                    JapaneseVoiceActorName = "Maeno Tomoaki",
                    JapaneseVoiceActorNativeName = "前野智昭",
                    AniListStaffId = 95041
                }
            };

            var credits = GenshinImpactCreditMapper.CreateCredits(staff, characters).ToList();

            Assert.AreEqual(1, credits.Count);
            Assert.AreEqual(BuiltInExtensionIds.GenshinImpact, credits[0].ExtensionId);
            Assert.AreEqual("Video Games", credits[0].CategoryName);
            Assert.AreEqual("Genshin Impact", credits[0].MediaTitle);
            Assert.AreEqual("Zhongli", credits[0].CharacterName);
        }

        [TestMethod]
        public void CreateCredits_WhenStaffNativeNameMatches_ReturnsGenshinCredit()
        {
            var staff = CreateStaff(fullName: "Manaka Iwami", nativeName: "石見舞菜香");
            var characters = new List<GenshinImpactCharacter>
            {
                new()
                {
                    Name = "Amber",
                    JapaneseVoiceActorName = "Iwami Manaka",
                    JapaneseVoiceActorNativeName = "石見舞菜香"
                }
            };

            var credits = GenshinImpactCreditMapper.CreateCredits(staff, characters).ToList();

            Assert.AreEqual(1, credits.Count);
            Assert.AreEqual("Amber", credits[0].CharacterName);
        }

        [TestMethod]
        public void CreateCredits_WhenStaffDoesNotMatch_ReturnsNoCredits()
        {
            var staff = CreateStaff(fullName: "Tomoaki Maeno", nativeName: "前野智昭");
            var characters = new List<GenshinImpactCharacter>
            {
                new()
                {
                    Name = "Amber",
                    JapaneseVoiceActorName = "Iwami Manaka",
                    JapaneseVoiceActorNativeName = "石見舞菜香"
                }
            };

            var credits = GenshinImpactCreditMapper.CreateCredits(staff, characters).ToList();

            Assert.AreEqual(0, credits.Count);
        }

        static Staff CreateStaff(string fullName, string nativeName) =>
            new(
                Id: 123,
                Name: new Names(
                    Romaji: fullName,
                    First: fullName.Split(' ').First(),
                    Last: fullName.Split(' ').Last(),
                    Full: fullName,
                    Native: nativeName,
                    Alternative: null,
                    AlternativeSpoiler: null),
                Language: Language.Japanese,
                Images: null,
                Description: null,
                Age: null,
                DateOfBirth: null,
                BloodType: null,
                SiteUrl: null,
                Characters: new List<Character>());
    }
}
