using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Trackify.Api.Models
{
    public class UserUpdate
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // User who receives the update
        public Guid UserId { get; set; }
        public User? User { get; set; }

        // Link to the related release (instead of UpdateLog)
        public Guid ReleaseId { get; set; }
        public Release? Release { get; set; }

        // Type of update: "NewRelease", "Reminder"
        public string Type { get; set; } = "NewRelease";

        // Status flags
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
    }
}
