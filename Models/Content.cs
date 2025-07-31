namespace Trackify.Api.Models
{
    public class Content
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "movie", "book", "game"
        public string Source { get; set; } = string.Empty; // ex: "IMDB", "Goodreads"
        public string Url { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
