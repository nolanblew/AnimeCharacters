using AniListClient.Models;
using System.Collections.Generic;

namespace AnimeCharacters.Comparers
{
    public class RoleComparer : IComparer<CharacterRole>
    {
        public static RoleComparer Instance { get; } = new();

        public int Compare(CharacterRole x, CharacterRole y)
        {
            if (x == y) { return 0; }
            return x switch
            {
                CharacterRole.Main => 1,
                CharacterRole.Background => -1,
                CharacterRole.Supporting => y == CharacterRole.Main ? -1 : 1,
                _ => -1
            };
        }
    }

    public class CharacterByRoleComparer : IComparer<Character>
    {
        public static CharacterByRoleComparer Instance { get; } = new();

        public int Compare(Character x, Character y)
        {
            if (x?.Role == null && y?.Role == null) { return 0; }
            if (x?.Role == null) { return -1; }
            if (y?.Role == null) { return 1; }

            return RoleComparer.Instance.Compare(x.Role.Value, y.Role.Value);
        }
    }
}
