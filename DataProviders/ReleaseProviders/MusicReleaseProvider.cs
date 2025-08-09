// MusicReleaseProvider.cs
using System.Text.Json;
using Trackify.Api.DataProviders.ReleaseProviders.Interfaces;
using Trackify.Api.Models;
using Trackify.Api.Services.Interfaces;

namespace Trackify.Api.DataProviders.ReleaseProviders
{
    public class MusicReleaseProvider : IMusicReleaseProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISpotifyAuthService _spotifyAuthService;

        public MusicReleaseProvider(IHttpClientFactory httpClientFactory, ISpotifyAuthService spotifyAuthService)
        {
            _httpClientFactory = httpClientFactory;
            _spotifyAuthService = spotifyAuthService;
        }

        public async Task<IEnumerable<Release>> GetSpotifyReleasesAsync(Entity entity)
        {
            var token = await _spotifyAuthService.GetSpotifyAccessTokenAsync();
            if (token == null) return Enumerable.Empty<Release>();

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var url = $"https://api.spotify.com/v1/artists/{entity.ExternalId}/albums?include_groups=album,single&limit=20";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode) return Enumerable.Empty<Release>();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("items").EnumerateArray().Select(album => new Release
            {
                Id = Guid.NewGuid(),
                EntityId = entity.Id,
                Title = album.GetProperty("name").GetString() ?? "Unknown",
                ReleaseDate = DateTime.TryParse(album.GetProperty("release_date").GetString(), out var dt) ? dt : DateTime.UtcNow,
                Url = album.GetProperty("external_urls").GetProperty("spotify").GetString(),
                Description = null
            });
        }

        public async Task<IEnumerable<Release>> GetMusicBrainzReleasesAsync(Entity entity)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://musicbrainz.org/ws/2/release-group?artist={entity.ExternalId}&type=album&fmt=json";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode) return Enumerable.Empty<Release>();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("release-groups").EnumerateArray().Select(rg => new Release
            {
                Id = Guid.NewGuid(),
                EntityId = entity.Id,
                Title = rg.GetProperty("title").GetString() ?? "Unknown",
                ReleaseDate = DateTime.UtcNow,
                Url = $"https://musicbrainz.org/release-group/{rg.GetProperty("id").GetString()}",
                Description = null
            });
        }
    }
}
