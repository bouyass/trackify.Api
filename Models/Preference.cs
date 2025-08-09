namespace Trackify.Api.Models
{
    public class Preference
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EntityId { get; set; }
        public Entity Entity { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserPreferenceLink> LinkedUsers { get; set; } = new List<UserPreferenceLink>();
    }
}
