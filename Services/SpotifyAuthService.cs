using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Trackify.Api.Services.Interfaces;

public class SpotifyAuthService : ISpotifyAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly string CLIENT_ID = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID") ?? throw new InvalidOperationException("SPOTIFY_CLIENT_ID is not set.");
    private static readonly string CLIENT_SECRET = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET") ?? throw new InvalidOperationException("SPOTIFY_CLIENT_SECRET is not set.");

    public SpotifyAuthService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string?> GetSpotifyAccessTokenAsync()
    {
        var client = _httpClientFactory.CreateClient();

        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{CLIENT_ID}:{CLIENT_SECRET}"));

        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
        request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Spotify Auth failed: {response.StatusCode}");
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("access_token").GetString();
    }
}
