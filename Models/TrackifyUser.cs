namespace Trackify.Api.Models
{
    // TrackifyDbContext
    public class TrackifyUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; } // FK to AuthDb.User.Id
        public User User { get; set; } = null!; // navigation if contexts share a DB

        public ICollection<Preference> Preferences { get; set; } = new List<Preference>();
        public ICollection<UserUpdate> Updates { get; set; } = new List<UserUpdate>();
    }

}
