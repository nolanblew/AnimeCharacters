using AniListClient.Responses;
using System.Linq;

namespace AniListClient.Converters
{
    internal static class CharacterResponseConverter
    {
        internal static Models.Media ToMedia(this Media media) =>
            new(
                Id: media.Id,
                Title: media.Title.ToTitle(),
                Description: media.Description,
                Image: media.CoverImage.ToImage(),
                Status: media.Status,
                Characters: media.Characters.Edges.Select(c => c.ToCharacter()).ToList());

        internal static Models.Character ToCharacter(this CharacterEdge characterEdge) =>
            new(
                Id: characterEdge.Node.Id,
                Name: characterEdge.Node.Name.ToCharacterName(),
                Image: characterEdge.Node.Image?.ToImage(),
                Role: characterEdge.Role,
                VoiceActors: characterEdge.VoiceActors.Select(va => va.ToVoiceActor()).ToList());

        internal static Models.VoiceActorSlim ToVoiceActor(this VoiceActor voiceActor) =>
            new(
                Id: voiceActor.Id,
                Name: voiceActor.Name.ToCharacterName());

        internal static Models.Images ToImage(this Image image) =>
            new(
                Medium: image.Medium?.AbsoluteUri,
                Large: image.Large?.AbsoluteUri,
                ExtraLarge: image.ExtraLarge?.AbsoluteUri,
                Color: image.Color);

        internal static Models.CharacterName ToCharacterName(this Name name) =>
            new Models.CharacterName(
                Romaji: name.Romaji,
                First: name.First,
                Last: name.Last,
                Full: name.Full,
                Native: name.Native,
                Alternative: name.Alternative,
                null);

        internal static Models.Titles ToTitle(this Title title) =>
            new(
                Romaji: title.Romaji,
                English: title.English,
                Native: title.Native,
                UserPreferred: title.UserPreferred);
    }
}
