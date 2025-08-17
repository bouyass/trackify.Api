using Google.Apis.Auth.OAuth2.Responses;
using System.Net.Http.Json;
using Trackify.Api.Dtos;
using Trackify.Api.Models;

namespace Trackify.Api.Services
{
    public interface IJwtService
    {
        Task<AuthResponseDto> GenerateToken(User user, Guid sessionId);
        Task<AuthResponseDto> RefreshToken(string refreshToken);
    }

    public class JwtService : IJwtService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public JwtService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<AuthResponseDto> GenerateToken(User user, Guid sessionId)
        {
            Console.WriteLine("Requesting token from Issuer ...");

            var issuerUrl = _config["Issuer:Url"] ?? _config["Url"]
                ?? throw new InvalidOperationException("Issuer:Url is not configured.");

            var appId = _config["Issuer:AppId"] ?? _config["AppId"]
                ?? throw new InvalidOperationException("Issuer:AppId is not configured.");

            var tenantId = _config["Issuer:TenantId"] ?? _config["AppId"]
                ?? throw new InvalidOperationException("Issuer:TenantId is not configured.");

            var adminApiKey = _config["Issuer:AdminApiKey"] ?? _config["AdminApiKey"]
                ?? throw new InvalidOperationException("Issuer:AdminApiKey is not configured.");

            var payload = new
            {
                appId,
                tenantId,
                sub = user.Id.ToString(),
                claims = new
                {
                    email = user.Email,
                    username = user.Username,
                    provider = user.Provider,
                    sid = sessionId.ToString(),
                    roles = new[] { "user" },
                    scope = "trackify:read"
                },
                ttl = "15m"
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{issuerUrl}/v1/tokens:mint")
            {
                Content = JsonContent.Create(payload)
            };

            request.Headers.Add("x-admin-api-key", adminApiKey);

            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Issuer token request failed. Status={response.StatusCode}, Error={error}");
            }

            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            return result ?? throw new InvalidOperationException("Issuer did not return tokens.");
        }


        public async Task<AuthResponseDto> RefreshToken(string refreshToken)
        {
            var appId = _config["Issuer:AppId"] ?? _config["AppId"];
            var issuerBase = _config["Issuer:Url"] ?? _config["Url"];

            var body = new
            {
                appId,
                refresh_token = refreshToken
            };

            var res = await _http.PostAsJsonAsync($"{issuerBase}/v1/tokens:refresh", body);
            res.EnsureSuccessStatusCode();

            return await res.Content.ReadFromJsonAsync<AuthResponseDto>()
                ?? throw new InvalidOperationException("Issuer returned no tokens.");
        }
    }
}
