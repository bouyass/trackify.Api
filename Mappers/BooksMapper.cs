namespace Trackify.Api.Mappers
{
    using System.Text.Json;
    using Trackify.Api.Mappers.Interfaces;
    using Trackify.Api.Models;

    public class BooksMapper : IBooksMapper
    {
        public IEnumerable<Entity> MapGoogleBooks(string json)
        {
            var results = new List<Entity>();
            if (string.IsNullOrWhiteSpace(json)) return results;

            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("items", out var items))
                return results;

            foreach (var item in items.EnumerateArray())
            {
                var volumeInfo = item.GetProperty("volumeInfo");

                results.Add(new Entity
                {
                    ExternalId = item.GetProperty("id").GetString() ?? "",
                    Name = volumeInfo.TryGetProperty("authors", out var authors) && authors.GetArrayLength() > 0
                           ? authors[0].GetString() ?? "Unknown"
                           : "Unknown",
                    Type = "Author",
                    Category = "Book",
                    Description = volumeInfo.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                    ImageUrl = volumeInfo.TryGetProperty("imageLinks", out var images) && images.TryGetProperty("thumbnail", out var thumb)
                                ? thumb.GetString()
                                : null
                });
            }

            return results;
        }

        public IEnumerable<Entity> MapOpenLibrary(string json)
        {
            var results = new List<Entity>();
            if (string.IsNullOrWhiteSpace(json)) return results;

            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("docs", out var docs))
                return results;

            foreach (var item in docs.EnumerateArray())
            {
                results.Add(new Entity
                {
                    ExternalId = item.GetProperty("key").GetString() ?? "",
                    Name = item.GetProperty("name").GetString() ?? "Unknown",
                    Type = "Author",
                    Category = "Book",
                    Description = item.TryGetProperty("top_work", out var topWork) ? topWork.GetString() : null
                });
            }

            return results;
        }
    }

}
