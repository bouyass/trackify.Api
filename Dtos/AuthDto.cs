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

    public class RefreshDto
    {
        public string RefreshToken { get; set; } = null!;
    }
}