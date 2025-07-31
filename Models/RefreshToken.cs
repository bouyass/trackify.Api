namespace Trackify.Api.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;

        // Relationship
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
