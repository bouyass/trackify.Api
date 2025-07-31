namespace Trackify.Api.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? PictureUrl { get; set; }
        public ICollection<UserPreference> Preferences { get; set; } = new List<UserPreference>();
        public ICollection<UserUpdate> Updates { get; set; } = new List<UserUpdate>();
        public ICollection<ExternalProvider> ExternalProviders { get; set; } = new List<ExternalProvider>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
