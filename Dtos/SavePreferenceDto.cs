namespace Trackify.Api.Dtos
{
    public class SavePreferenceDto
    {
        public string ExternalId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Type { get; set; } = null!; // book, song, event, etc.
        public string? Category { get; set; }
        public string? Description { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? Source { get; set; } // e.g., "IMDB", "Goodreads"
        public string? Url { get; set; } // URL to the content
        public string? Author { get; set; } // For books
        public string? ISBN { get; set; } // For books
        public string? Artist { get; set; } // For songs
        public string? Album { get; set; } // For songs
        public string? Location { get; set; } // For events
        public DateTime? EventDate { get; set; } // For events  
    }
}