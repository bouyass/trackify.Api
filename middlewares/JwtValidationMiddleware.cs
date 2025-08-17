using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Trackify.Api.middlewares
{
    public class JwtValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly ILogger<JwtValidationMiddleware> _logger;

        public JwtValidationMiddleware(
            RequestDelegate next,
            IConfiguration config,
            HttpClient http,
            IMemoryCache cache,
            ILogger<JwtValidationMiddleware> logger)
        {
            _next = next;
            _config = config;
            _http = http;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrWhiteSpace(token))
            {
                await _next(context);
                return;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var kid = jwt.Header.Kid;
                if (string.IsNullOrEmpty(kid))
                    throw new SecurityTokenException("Missing kid in JWT header");

                var keys = await GetIssuerKeysAsync();
                var key = keys.FirstOrDefault(k => k.PublicJwk.Kid == kid && k.IsActive);
                if (key == null)
                    throw new SecurityTokenException("Unknown or inactive key");

                // Build RSA from JWK (n, e are base64url encoded)
                var rsa = RSA.Create();
                rsa.ImportParameters(new RSAParameters
                {
                    Modulus = Base64UrlDecode(key.PublicJwk.N),
                    Exponent = Base64UrlDecode(key.PublicJwk.E)
                });

                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _config["Issuer:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = _config["Issuer:Audience"],

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(rsa) { KeyId = key.PublicJwk.Kid },

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                };

                var principal = handler.ValidateToken(token, parameters, out _);
                context.User = principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "JWT validation failed");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            await _next(context);
        }

        // Helper for base64url decoding
        private static byte[] Base64UrlDecode(string input)
        {
      
            if (input.Length % 4 == 2) input += "==";
            else if (input.Length % 4 == 3) input += "=";

            input = input.Replace('-', '+').Replace('_', '/');
            return Convert.FromBase64String(input);
        }

        private async Task<List<IssuerKeyDto>> GetIssuerKeysAsync()
        {
            return await _cache.GetOrCreateAsync("issuer_keys", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                var issuerBase = _config["Issuer:Url"];
                var appId = _config["Issuer:AppId"];
                var adminKey = _config["Issuer:AdminApiKey"];

                var req = new HttpRequestMessage(HttpMethod.Get, $"{issuerBase}/v1/apps/{appId}/keys");
                req.Headers.Add("x-admin-api-key", adminKey);

                var res = await _http.SendAsync(req);
                res.EnsureSuccessStatusCode();

                var keys = await res.Content.ReadFromJsonAsync<List<IssuerKeyDto>>();
                return keys ?? new List<IssuerKeyDto>();
            }) ?? new List<IssuerKeyDto>();
        }
    }

    public class IssuerKeyDto
    {
        public string Id { get; set; } = default!;
        public string AppId { get; set; } = default!;
        public string Kid { get; set; } = default!;
        public bool IsActive { get; set; }
        public DateTime? NotBefore { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public PublicJwkDto PublicJwk { get; set; } = default!;
    }

    public class PublicJwkDto
    {
        public string Kty { get; set; } = default!;
        public string Kid { get; set; } = default!;
        public string N { get; set; } = default!;  // modulus, base64url string
        public string E { get; set; } = default!;  // exponent, base64url string
        public string Alg { get; set; } = default!;
        public string Use { get; set; } = default!;
    }
}
