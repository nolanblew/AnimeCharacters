using AniListClient.Models;
using Kitsu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ReferenceApis
{
    public class JikanReferenceAnimeProvider : IReferenceAnimeProvider
    {
        const string _BASE_URL = "https://api.jikan.moe/v4/";
        static readonly TimeSpan _DEFAULT_REQUEST_TIMEOUT = TimeSpan.FromSeconds(15);
        static readonly TimeSpan _DEFAULT_RETRY_DELAY = TimeSpan.FromMilliseconds(250);
        static readonly TimeSpan _MAX_RETRY_DELAY = TimeSpan.FromSeconds(2);
        const int _MAX_REQUEST_ATTEMPTS = 3;
        readonly HttpClient _httpClient;
        readonly TimeSpan _requestTimeout;
        readonly Uri _baseUri;
        readonly string _providerName;
        readonly string _displayName;

        public JikanReferenceAnimeProvider(
            HttpClient httpClient,
            TimeSpan? requestTimeout = null,
            Uri baseUri = null,
            string providerName = null,
            string displayName = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _requestTimeout = requestTimeout ?? _DEFAULT_REQUEST_TIMEOUT;
            _baseUri = NormalizeBaseUri(baseUri ?? new Uri(_BASE_URL));
            _providerName = string.IsNullOrWhiteSpace(providerName)
                ? ReferenceProviderNames.Jikan
                : providerName.Trim();
            _displayName = string.IsNullOrWhiteSpace(displayName)
                ? (_providerName == ReferenceProviderNames.Jikan ? "Jikan" : _providerName)
                : displayName.Trim();
        }

        public string Name => _providerName;
        public string DisplayName => _displayName;

        public ReferenceAnimeKey GetKnownAnimeKey(Anime anime) =>
            !string.IsNullOrWhiteSpace(anime?.MyAnimeListId)
                ? new ReferenceAnimeKey(Name, anime.MyAnimeListId)
                : null;

        public async Task<ReferenceMediaResult> GetMediaWithCharactersAsync(
            Anime anime,
            IReadOnlyCollection<string> searchTitles)
        {
            var animeId = anime?.MyAnimeListId;

            if (string.IsNullOrWhiteSpace(animeId))
            {
                animeId = await SearchAnimeIdAsync(searchTitles);
            }

            if (string.IsNullOrWhiteSpace(animeId) || !int.TryParse(animeId, out var id))
            {
                throw new ReferenceApiProviderException($"{DisplayName} could not find a matching MyAnimeList anime id.");
            }

            var response = await GetFromJsonAsync<JikanDataResponse<List<JikanAnimeCharacterEntry>>>(
                $"anime/{id}/characters");

            var characters = response?.Data?
                .Select(ToCharacter)
                .Where(character => character != null)
                .ToList() ?? new List<Character>();

            return new ReferenceMediaResult(
                new ReferenceAnimeKey(Name, animeId),
                new Media(
                    Id: id,
                    Title: ToTitles(anime),
                    Description: null,
                    Image: ToImages(anime?.PosterImageUrl),
                    Status: MediaStatus.Finished,
                    Characters: characters,
                    ProviderName: Name));
        }

        public async Task<Staff> GetStaffByIdAsync(string id)
        {
            if (!int.TryParse(id, out var staffId))
            {
                throw new ReferenceApiProviderException($"{DisplayName} requires a numeric MyAnimeList person id.");
            }

            var response = await GetFromJsonAsync<JikanDataResponse<JikanPerson>>($"people/{staffId}/full");
            var person = response?.Data;

            if (person == null)
            {
                throw new ReferenceApiProviderException($"{DisplayName} did not return person data.");
            }

            return new Staff(
                Id: person.MalId,
                Name: ToNames(person.Name),
                Language: Language.Japanese,
                Images: ToImages(person.Images?.Jpg?.ImageUrl),
                Description: person.About,
                Age: null,
                DateOfBirth: ToDateOfBirth(person.Birthday),
                BloodType: ExtractBloodType(person.About),
                SiteUrl: person.Url,
                Characters: person.Voices?.Select(ToCharacter).Where(character => character != null).ToList() ?? new List<Character>(),
                ProviderName: Name);
        }

        public async Task<Staff> FindStaffByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var response = await GetFromJsonAsync<JikanDataResponse<List<JikanPerson>>>(
                $"people?q={Uri.EscapeDataString(name)}&limit=10");
            var match = response?.Data?
                .FirstOrDefault(person => StaffNameMatcher.IsExactMatch(ToNames(person?.Name), name));

            return match == null
                ? null
                : await GetStaffByIdAsync(match.MalId.ToString());
        }

        async Task<string> SearchAnimeIdAsync(IReadOnlyCollection<string> searchTitles)
        {
            foreach (var title in searchTitles ?? Array.Empty<string>())
            {
                var response = await GetFromJsonAsync<JikanDataResponse<List<JikanAnimeSearchResult>>>(
                    $"anime?q={Uri.EscapeDataString(title)}&limit=5");

                var match = response?.Data?.FirstOrDefault(result => _IsTitleMatch(result, searchTitles));
                if (match != null)
                {
                    return match.MalId.ToString();
                }
            }

            return null;
        }

        async Task<T> GetFromJsonAsync<T>(string relativeUrl)
        {
            using var cancellation = new CancellationTokenSource(_requestTimeout);
            HttpRequestException lastRequestException = null;

            try
            {
                for (var attempt = 1; attempt <= _MAX_REQUEST_ATTEMPTS; attempt++)
                {
                    try
                    {
                        using var response = await _httpClient.GetAsync(
                            new Uri(_baseUri, relativeUrl),
                            cancellation.Token);

                        if (IsTransient(response.StatusCode) && attempt < _MAX_REQUEST_ATTEMPTS)
                        {
                            await Task.Delay(GetRetryDelay(response, attempt), cancellation.Token);
                            continue;
                        }

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new ReferenceApiProviderException(
                                $"{DisplayName} returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).");
                        }

                        try
                        {
                            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellation.Token);
                        }
                        catch (Exception ex) when (ex is JsonException || ex is NotSupportedException)
                        {
                            throw new ReferenceApiProviderException($"{DisplayName} returned an invalid response.", ex);
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        lastRequestException = ex;

                        if (attempt == _MAX_REQUEST_ATTEMPTS)
                        {
                            break;
                        }

                        await Task.Delay(TimeSpan.FromMilliseconds(_DEFAULT_RETRY_DELAY.TotalMilliseconds * attempt), cancellation.Token);
                    }
                }
            }
            catch (OperationCanceledException ex) when (cancellation.IsCancellationRequested)
            {
                throw new ReferenceApiProviderException($"{DisplayName} request timed out.", ex);
            }

            throw new ReferenceApiProviderException($"{DisplayName} could not be reached after retrying.", lastRequestException);
        }

        static Uri NormalizeBaseUri(Uri baseUri)
        {
            if (!baseUri.IsAbsoluteUri)
            {
                throw new ArgumentException("The provider base URI must be absolute.", nameof(baseUri));
            }

            var value = baseUri.AbsoluteUri;
            return value.EndsWith('/') ? baseUri : new Uri($"{value}/");
        }

        static bool IsTransient(HttpStatusCode statusCode) =>
            statusCode == HttpStatusCode.RequestTimeout
            || statusCode == HttpStatusCode.TooManyRequests
            || (int)statusCode >= 500;

        static TimeSpan GetRetryDelay(HttpResponseMessage response, int attempt)
        {
            var retryAfter = response.Headers.RetryAfter;

            if (retryAfter?.Delta is TimeSpan delta && delta > TimeSpan.Zero)
            {
                return Min(delta, _MAX_RETRY_DELAY);
            }

            if (retryAfter?.Date is DateTimeOffset date)
            {
                var delay = date - DateTimeOffset.UtcNow;
                if (delay > TimeSpan.Zero)
                {
                    return Min(delay, _MAX_RETRY_DELAY);
                }
            }

            return TimeSpan.FromMilliseconds(_DEFAULT_RETRY_DELAY.TotalMilliseconds * attempt);
        }

        static TimeSpan Min(TimeSpan value, TimeSpan maximum) =>
            value <= maximum ? value : maximum;

        Character ToCharacter(JikanAnimeCharacterEntry entry)
        {
            if (entry?.Character == null)
            {
                return null;
            }

            var voiceActors = entry.VoiceActors?
                .Where(voiceActor => string.Equals(voiceActor.Language, "Japanese", StringComparison.OrdinalIgnoreCase))
                .Select(ToVoiceActorSlim)
                .Where(voiceActor => voiceActor != null)
                .ToList() ?? new List<VoiceActorSlim>();

            return new Character(
                Id: entry.Character.MalId,
                Name: ToNames(entry.Character.Name),
                Image: ToImages(entry.Character.Images?.Jpg?.ImageUrl),
                Description: null,
                Role: ToCharacterRole(entry.Role),
                Media: null,
                VoiceActors: voiceActors,
                ProviderName: Name);
        }

        Character ToCharacter(JikanPersonVoice entry)
        {
            if (entry?.Character == null || entry.Anime == null)
            {
                return null;
            }

            return new Character(
                Id: entry.Character.MalId,
                Name: ToNames(entry.Character.Name),
                Image: ToImages(entry.Character.Images?.Jpg?.ImageUrl),
                Description: null,
                Role: ToCharacterRole(entry.Role),
                Media: new List<MediaBase>
                {
                    new MediaBase(
                        Id: entry.Anime.MalId,
                        Title: ToTitles(entry.Anime.Title),
                        ProviderName: Name)
                },
                VoiceActors: null,
                ProviderName: Name);
        }

        VoiceActorSlim ToVoiceActorSlim(JikanVoiceActor voiceActor)
        {
            if (voiceActor?.Person == null)
            {
                return null;
            }

            return new VoiceActorSlim(
                Id: voiceActor.Person.MalId,
                Name: ToNames(voiceActor.Person.Name),
                ProviderName: Name,
                SiteUrl: voiceActor.Person.Url);
        }

        static CharacterRole? ToCharacterRole(string role) =>
            role?.Trim().ToLowerInvariant() switch
            {
                "main" => CharacterRole.Main,
                "supporting" => CharacterRole.Supporting,
                "background" => CharacterRole.Background,
                _ => null
            };

        static Names ToNames(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new Names(null, null, null, null, null, null, null);
            }

            var trimmedName = name.Trim();
            var first = trimmedName;
            string last = null;

            if (trimmedName.Contains(','))
            {
                var parts = trimmedName.Split(',', 2, StringSplitOptions.TrimEntries);
                last = parts[0];
                first = parts.Length > 1 ? parts[1] : null;
                trimmedName = string.Join(" ", new[] { first, last }.Where(part => !string.IsNullOrWhiteSpace(part)));
            }

            return new Names(
                Romaji: trimmedName,
                First: first,
                Last: last,
                Full: trimmedName,
                Native: null,
                Alternative: null,
                AlternativeSpoiler: null);
        }

        static Titles ToTitles(Anime anime) =>
            new Titles(
                Romaji: anime?.RomanjiTitle ?? anime?.Title,
                English: anime?.EnglishTitle,
                Native: null,
                UserPreferred: anime?.Title ?? anime?.RomanjiTitle ?? anime?.EnglishTitle);

        static Titles ToTitles(string title) =>
            new Titles(
                Romaji: title,
                English: title,
                Native: null,
                UserPreferred: title);

        static Images ToImages(string imageUrl) =>
            new Images(
                Medium: imageUrl,
                Large: imageUrl,
                ExtraLarge: imageUrl,
                Color: null);

        static DateOfBirth ToDateOfBirth(string birthday)
        {
            if (!DateTimeOffset.TryParse(birthday, out var date))
            {
                return null;
            }

            return new DateOfBirth(date.Year, date.Month, date.Day);
        }

        static string ExtractBloodType(string about)
        {
            if (string.IsNullOrWhiteSpace(about))
            {
                return null;
            }

            var match = Regex.Match(about, @"Blood\s+type:\s*(?<blood>[^\r\n]+)", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups["blood"].Value.Trim() : null;
        }

        static bool _IsTitleMatch(JikanAnimeSearchResult result, IEnumerable<string> searchTitles)
        {
            var normalizedSearchTitles = searchTitles
                .Select(_NormalizeTitle)
                .Where(title => !string.IsNullOrWhiteSpace(title))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var candidateTitles = new[]
            {
                result.Title,
                result.TitleEnglish,
                result.TitleJapanese
            }.Concat(result.TitleSynonyms ?? Array.Empty<string>());

            return candidateTitles
                .Select(_NormalizeTitle)
                .Any(title => !string.IsNullOrWhiteSpace(title) && normalizedSearchTitles.Contains(title));
        }

        static string _NormalizeTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            return Regex.Replace(title, @"[^\p{L}\p{N}]+", " ").Trim();
        }

        class JikanDataResponse<T>
        {
            [JsonPropertyName("data")]
            public T Data { get; set; }
        }

        class JikanAnimeCharacterEntry
        {
            [JsonPropertyName("character")]
            public JikanResource Character { get; set; }

            [JsonPropertyName("role")]
            public string Role { get; set; }

            [JsonPropertyName("voice_actors")]
            public List<JikanVoiceActor> VoiceActors { get; set; }
        }

        class JikanVoiceActor
        {
            [JsonPropertyName("person")]
            public JikanResource Person { get; set; }

            [JsonPropertyName("language")]
            public string Language { get; set; }
        }

        class JikanPerson
        {
            [JsonPropertyName("mal_id")]
            public int MalId { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("images")]
            public JikanImages Images { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("about")]
            public string About { get; set; }

            [JsonPropertyName("birthday")]
            public string Birthday { get; set; }

            [JsonPropertyName("voices")]
            public List<JikanPersonVoice> Voices { get; set; }
        }

        class JikanPersonVoice
        {
            [JsonPropertyName("role")]
            public string Role { get; set; }

            [JsonPropertyName("anime")]
            public JikanResource Anime { get; set; }

            [JsonPropertyName("character")]
            public JikanResource Character { get; set; }
        }

        class JikanAnimeSearchResult
        {
            [JsonPropertyName("mal_id")]
            public int MalId { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("title_english")]
            public string TitleEnglish { get; set; }

            [JsonPropertyName("title_japanese")]
            public string TitleJapanese { get; set; }

            [JsonPropertyName("title_synonyms")]
            public string[] TitleSynonyms { get; set; }
        }

        class JikanResource
        {
            [JsonPropertyName("mal_id")]
            public int MalId { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("images")]
            public JikanImages Images { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }
        }

        class JikanImages
        {
            [JsonPropertyName("jpg")]
            public JikanJpgImage Jpg { get; set; }
        }

        class JikanJpgImage
        {
            [JsonPropertyName("image_url")]
            public string ImageUrl { get; set; }

            [JsonPropertyName("large_image_url")]
            public string LargeImageUrl { get; set; }
        }
    }
}
