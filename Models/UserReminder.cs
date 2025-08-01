namespace Trackify.Api.Models
{
    public class UserReminder
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ReleaseId { get; set; }
        public DateTime ReminderDate { get; set; }
        public bool Notified { get; set; } = false;

        public User User { get; set; } = null!;
        public Release Release { get; set; } = null!;
    }

}
