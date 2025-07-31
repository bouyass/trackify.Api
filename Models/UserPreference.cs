namespace Trackify.Api.Models
{
    public class UserPreference
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User? User { get; set; }

        // Lien vers le contenu suivi
        public Guid ContentId { get; set; }
        public Content? Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
