namespace Trackify.Api.Models
{
    public class UserPreference
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Category { get; set; } = string.Empty; // ex: "movie", "book", "game"
        public string Keywords { get; set; } = string.Empty; // ex: "fantasy, marvel, thriller"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? User { get; set; }
    }
}
