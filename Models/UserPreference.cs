namespace Trackify.Api.Models
{
    public class UserPreference
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid EntityId { get; set; }

        public User User { get; set; } = null!;
        public Entity Entity { get; set; } = null!;
    }
}
