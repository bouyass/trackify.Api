using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Trackify.Api.Models
{
    public class UserUpdate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public Guid UpdateLogId { get; set; }
        public UpdateLog? UpdateLog { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
    }

}
