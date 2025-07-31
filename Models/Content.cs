namespace Trackify.Api.Models
{
    public class Content
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty; // "IMDB", "Goodreads"
        public string Url { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<UpdateLog> Updates { get; set; } = new List<UpdateLog>();

        // Lien vers la catégorie
        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
