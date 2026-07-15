using AniListClient.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReferenceApis
{
    static class StaffNameMatcher
    {
        public static bool IsExactMatch(Names names, string expectedName)
        {
            var expected = Normalize(expectedName);
            if (string.IsNullOrWhiteSpace(expected) || names == null)
            {
                return false;
            }

            return GetCandidateNames(names)
                .Select(Normalize)
                .Any(candidate => candidate == expected);
        }

        static IEnumerable<string> GetCandidateNames(Names names)
        {
            yield return names.Full;
            yield return names.Romaji;
            yield return names.Native;
            yield return $"{names.First} {names.Last}";
            yield return $"{names.Last} {names.First}";
        }

        static string Normalize(string name) =>
            string.IsNullOrWhiteSpace(name)
                ? null
                : Regex.Replace(name, @"[^\p{L}\p{N}]+", string.Empty).ToLowerInvariant();
    }
}
