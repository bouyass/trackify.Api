using Trackify.Api.Models;

public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty; // ex: "Movie", "Book", "Game"
        public string? Description { get; set; }
        public string? Icon { get; set; } // URL ou nom d'icône pour l'app

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relations
        public ICollection<Content> Contents { get; set; } = new List<Content>();
    }