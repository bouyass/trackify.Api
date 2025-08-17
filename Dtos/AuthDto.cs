using System.Text.Json.Serialization;

namespace Trackify.Api.Dtos
{
    public class AuthDto
    {
        public string IdToken { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginDto
    {
        public string Identifier { get; set; } = null!; // username or email
        public string Password { get; set; } = null!;
    }


    public class LogoutDto
    {
        public string RefreshToken { get; set; } = null!;
    }


    public class RefreshDto
    {
        public string RefreshToken { get; set; } = null!;
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? PictureUrl { get; set; }
        public string Provider { get; set; } = null!;
        public string? Locale { get; set; }
    }

    public class AuthResponseDto
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = null!;
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = null!;
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        public Guid SessionId { get; set; }
        public UserDto User { get; set; } = null!;
    }
}