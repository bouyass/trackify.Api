namespace Trackify.Api.Models
{
    public class UpdateLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ItemId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Content? Item { get; set; }
    }
}
