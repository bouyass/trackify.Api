namespace Trackify.Api.Models
{
    public class Entity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = null!; // ID from API (Spotify, TMDB, etc.)
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!; // "Author", "Artist", "Studio", "Organizer"
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Category { get; set; } // e.g., "Book", "Song", "Movie", "Event"
    public string? AuthorOrArtist { get; set; }
    public ICollection<Release> Releases { get; set; } = new List<Release>();
}
}
