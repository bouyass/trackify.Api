namespace Trackify.Api.Models
{
    public class UserPreferenceLink
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TrackifyUserId { get; set; }
        public TrackifyUser TrackifyUser { get; set; } = null!;

        public Guid PreferenceId { get; set; }
        public Preference Preference { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
