namespace Trackify.Api.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<UserPreference> Preferences { get; set; } = new List<UserPreference>();
        public ICollection<UserUpdate> Updates { get; set; } = new List<UserUpdate>();
    }
}
