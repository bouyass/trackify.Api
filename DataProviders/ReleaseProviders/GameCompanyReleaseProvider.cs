using System.Text.Json;
using Trackify.Api.DataProviders.ReleaseProviders.Interfaces;
using Trackify.Api.Models;

namespace Trackify.Api.DataProviders.ReleaseProviders
{
    public class GameCompanyReleaseProvider : IGameCompanyReleaseProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GameCompanyReleaseProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<Release>> GetRawgReleasesAsync(Entity entity)
        {
            var client = _httpClientFactory.CreateClient();
            var apiKey = Environment.GetEnvironmentVariable("RAWG_API_KEY") ?? throw new InvalidOperationException("RAWG_API_KEY not set");
            var url = $"https://api.rawg.io/api/games?developers={Uri.EscapeDataString(entity.Name)}&key={apiKey}";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return Enumerable.Empty<Release>();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("results").EnumerateArray().Select(game => new Release
            {
                Id = Guid.NewGuid(),
                EntityId = entity.Id,
                Title = game.GetProperty("name").GetString() ?? "Unknown",
                ReleaseDate = DateTime.TryParse(game.GetProperty("released").GetString(), out var dt) ? dt : DateTime.UtcNow,
                Url = game.GetProperty("slug").GetString() is string slug ? $"https://rawg.io/games/{slug}" : null,
                Description = game.TryGetProperty("description", out var desc) ? desc.GetString() : null
            });
        }

        public async Task<IEnumerable<Release>> GetIGDBReleasesAsync(Entity entity)
        {
            var clientId = Environment.GetEnvironmentVariable("IGDB_CLIENT_ID")
                           ?? throw new InvalidOperationException("IGDB_CLIENT_ID not set");
            var clientSecret = Environment.GetEnvironmentVariable("IGDB_CLIENT_SECRET")
                               ?? throw new InvalidOperationException("IGDB_CLIENT_SECRET not set");

            // Get access token via Twitch
            var authClient = _httpClientFactory.CreateClient();
            var tokenResponse = await authClient.PostAsync(
                $"https://id.twitch.tv/oauth2/token?client_id={clientId}&client_secret={clientSecret}&grant_type=client_credentials",
                null);

            if (!tokenResponse.IsSuccessStatusCode) return Enumerable.Empty<Release>();

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            using var tokenDoc = JsonDocument.Parse(tokenJson);
            var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString();

            // Make IGDB request
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Client-ID", clientId);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var body = $@"
            fields name,first_release_date,url,slug;
            where involved_companies.company.name = ""{entity.Name.Replace("\"", "\\\"")}"" & first_release_date != null;
            sort first_release_date desc;
            limit 20;";

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.igdb.com/v4/games")
            {
                Content = new StringContent(body)
            };
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return Enumerable.Empty<Release>();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.EnumerateArray().Select(game => new Release
            {
                Id = Guid.NewGuid(),
                EntityId = entity.Id,
                Title = game.GetProperty("name").GetString() ?? "Unknown",
                ReleaseDate = game.TryGetProperty("first_release_date", out var dateProp)
                    ? DateTimeOffset.FromUnixTimeSeconds(dateProp.GetInt64()).UtcDateTime
                    : DateTime.UtcNow,
                Url = game.TryGetProperty("slug", out var slug) ? $"https://www.igdb.com/games/{slug.GetString()}" : null,
                Description = null
            });
        }

    }
}
