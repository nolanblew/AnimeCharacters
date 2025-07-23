using AniListClient.Responses;
using AniListClient.Responses.AniListClient.Responses;
using System.Linq;
using Models = AniListClient.Models;

namespace AniListClient.Converters
{
    internal static class CharacterResponseConverter
    {
        internal static Models.Media ToMedia(this CharactersFromAnimeResponse.MediaResponse media) =>
            new(
                Id: media.Id,
                Title: media.Title.ToTitle(),
                Description: media.Description,
                Image: media.CoverImage.ToImage(),
                Status: media.Status,
                Characters: media.Characters.Edges.Select(c => c.ToCharacter()).ToList());

        internal static Models.MediaBase ToMedia(this StaffQueryResponse.MediaResponse media) =>
            new(
                Id: media.Id,
                Title: media.Title.ToTitle());

        internal static Models.Character ToCharacter(this CharactersFromAnimeResponse.CharacterEdge characterEdge) =>
            new(
                Id: characterEdge.Node.Id,
                Name: characterEdge.Node.Name.ToCharacterName(),
                Image: characterEdge.Node.Image?.ToImage(),
                Description: null,
                Role: characterEdge.Role,
                Media: null,
                VoiceActors: characterEdge.VoiceActors.Select(va => va.ToVoiceActor()).ToList());

        internal static Models.Character ToCharacter(this StaffQueryResponse.CharacterEdgeResponse characterEdge) =>
            new(
                Id: characterEdge.Id,
                Name: characterEdge.Node.Name.ToCharacterName(),
                Image: characterEdge.Node.Image?.ToImage(),
                Description: characterEdge.Node.Description,
                Role: characterEdge.Role,
                Media: characterEdge.Media.Select(m => m.ToMedia()).ToList(),
                VoiceActors: null);

        internal static Models.VoiceActorSlim ToVoiceActor(this CharactersFromAnimeResponse.VoiceActorResponse voiceActor) =>
            new(
                Id: voiceActor.Id,
                Name: voiceActor.Name.ToCharacterName());

        internal static Models.Staff ToStaff(this StaffQueryResponse.StaffResponse staff) =>
            new(
                Id: staff.Id,
                Name: staff.Name.ToCharacterName(),
                Language: staff.LanguageV2,
                Images: staff.Image.ToImage(),
                Age: staff.Age,
                DateOfBirth: staff.DateOfBirth?.ToFuzzyDate(),
                BloodType: staff.BloodType,
                Description: staff.Description,
                Characters: staff.Characters.Edges.Select(c => c.ToCharacter()).ToList());

        internal static Models.FuzzyDate ToFuzzyDate(this Responses.FuzzyDate date) =>
            new(
                Year: date.Year,
                Month: date.Month,
                Day: date.Day);

        internal static Models.Images ToImage(this Image image) =>
            new(
                Medium: image.Medium?.AbsoluteUri,
                Large: image.Large?.AbsoluteUri,
                ExtraLarge: image.ExtraLarge?.AbsoluteUri,
                Color: image.Color);

        internal static Models.Names ToCharacterName(this Name name) =>
            new Models.Names(
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
