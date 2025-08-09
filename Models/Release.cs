namespace Trackify.Api.Models
{
    public class Release
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
        public string ExternalId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Type { get; set; } = null!; // "Book", "Song", "Album", "Movie", "Event"
        public string? Description { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? Location { get; set; } // For events
        public string? CoverImageUrl { get; set; }
        public string? Url { get; set; } // Link to more details (e.g., Google Books, Spotify, etc.)
        public Entity Entity { get; set; } = null!;
    }
}
