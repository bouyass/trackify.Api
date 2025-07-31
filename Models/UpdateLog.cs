namespace Trackify.Api.Models
{
    public class UpdateLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ContentId { get; set; }
        public Content? Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<UserUpdate> UserUpdates { get; set; } = new List<UserUpdate>();
    }
}
