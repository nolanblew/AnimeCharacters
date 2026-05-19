using AnimeCharacters.Models;
using AniListClient.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AnimeCharacters.Extensions.GenshinImpact
{
    public static class GenshinImpactCreditMapper
    {
        public static IReadOnlyList<VoiceActorCreditModel> CreateCredits(
            Staff staff,
            IEnumerable<GenshinImpactCharacter> characters)
        {
            if (staff?.Name == null || characters == null)
            {
                return new List<VoiceActorCreditModel>();
            }

            return characters
                .SelectMany(character => character.GetJapaneseVoiceActors()
                    .Where(voiceActor => Matches(staff.Name, voiceActor))
                    .Select(voiceActor => CreateCredit(character, voiceActor)))
                .OrderBy(credit => credit.CharacterName)
                .ToList();
        }

        static VoiceActorCreditModel CreateCredit(GenshinImpactCharacter character, GenshinImpactVoiceActor voiceActor) =>
            new()
            {
                ExtensionId = BuiltInExtensionIds.GenshinImpact,
                ExtensionName = "Genshin Impact",
                CategoryName = "Video Games",
                MediaId = BuiltInExtensionIds.GenshinImpact,
                MediaTitle = "Genshin Impact",
                MediaImageUrl = "images/extensions/genshin-impact.svg",
                MediaRoute = "/extensions/genshin-impact",
                CharacterId = character.Name,
                CharacterName = character.Name,
                CharacterImageUrl = character.ImageUrl,
                CharacterRoute = voiceActor.AniListStaffId.HasValue ? $"/characters/{voiceActor.AniListStaffId.Value}" : null,
                ExternalUrl = character.WikiUrl
            };

        public static bool Matches(Names staffName, GenshinImpactVoiceActor voiceActor)
        {
            var staffNames = GetComparableNames(staffName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var voiceActorNames = GetComparableNames(voiceActor).ToList();

            return voiceActorNames.Any(staffNames.Contains);
        }

        static IEnumerable<string> GetComparableNames(Names staffName)
        {
            foreach (var name in NormalizeMany(staffName.Full, staffName.Native, staffName.Romaji))
            {
                yield return name;
            }

            foreach (var name in NormalizeNameOrders(staffName.First, staffName.Last))
            {
                yield return name;
            }
        }

        static IEnumerable<string> GetComparableNames(GenshinImpactVoiceActor voiceActor)
        {
            foreach (var name in NormalizeMany(voiceActor.Name, voiceActor.NativeName))
            {
                yield return name;
            }

            var parts = voiceActor.Name?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts?.Length == 2)
            {
                foreach (var name in NormalizeNameOrders(parts[0], parts[1]))
                {
                    yield return name;
                }
            }
        }

        static IEnumerable<string> NormalizeNameOrders(string first, string last) =>
            NormalizeMany(
                string.Join(" ", new[] { first, last }.Where(part => !string.IsNullOrWhiteSpace(part))),
                string.Join(" ", new[] { last, first }.Where(part => !string.IsNullOrWhiteSpace(part))));

        static IEnumerable<string> NormalizeMany(params string[] values) =>
            values
                .Select(Normalize)
                .Where(value => !string.IsNullOrWhiteSpace(value));

        static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            return Regex.Replace(builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant(), @"[^\p{L}\p{N}]+", " ").Trim();
        }
    }
}
