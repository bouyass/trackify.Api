using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using System.Text.Json;
using Trackify.Api.DataProviders.EntityProviders.Interfaces;
using Trackify.Api.Services.Interfaces;

namespace Trackify.Api.DataProviders.EntityProviders
{
    public class MusicEntityProvider : IExternalMusicEntityProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISpotifyAuthService _spotifyAuthService;

        public MusicEntityProvider(IHttpClientFactory httpClientFactory, ISpotifyAuthService spotifyAuthService)
        {
            _httpClientFactory = httpClientFactory;
            _spotifyAuthService = spotifyAuthService;
        }

        public async Task<string> SpotifyMusicProvider(string query, int page, int pageSize)
        
        {
            var token = await _spotifyAuthService.GetSpotifyAccessTokenAsync();
            if (token == null) return string.Empty;


            var offset = (page - 1) * pageSize;

            if (string.IsNullOrWhiteSpace(query))
            {
                query = "a";
            }


            var url = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=artist&limit={pageSize}&offset={offset}";
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Spotify error: {ex.Message}");
            }

            return string.Empty;
        }

        public async Task<string> MusicBrainzProvider(string query, int page, int pageSize)
        {
            var client = _httpClientFactory.CreateClient();
            var offset = (page - 1) * pageSize;
            var musicBrainzUrl = $"https://musicbrainz.org/ws/2/artist?query={Uri.EscapeDataString(query)}&fmt=json&limit={pageSize}&offset={offset}";
            try
            {
                var response = await client.GetAsync(musicBrainzUrl);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching MusicBrainz data: {ex.Message}");
            }
            return string.Empty;
        }
    }
}
