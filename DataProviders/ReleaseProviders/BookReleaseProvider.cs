using System.Globalization;
using System.Text.Json;
using Trackify.Api.DataProviders.ReleaseProviders.Interfaces;
using Trackify.Api.Models;

namespace Trackify.Api.DataProviders.ReleaseProviders
{
    public class BookReleaseProvider : IBookReleaseProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BookReleaseProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<Release>> GetGoogleBooksReleasesAsync(Entity author)
        {
            var releases = new List<Release>();
            if (string.IsNullOrWhiteSpace(author.Name)) return releases;

            var client = _httpClientFactory.CreateClient();
            var url = $"https://www.googleapis.com/books/v1/volumes?q=inauthor:{Uri.EscapeDataString(author.Name)}&orderBy=newest&maxResults=20";

            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return releases;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("items", out var items)) return releases;

                foreach (var item in items.EnumerateArray())
                {
                    var info = item.GetProperty("volumeInfo");

                    DateTime? pubDate = null;
                    if (info.TryGetProperty("publishedDate", out var pubDateProp))
                        pubDate = ParseDate(pubDateProp.GetString());

                    releases.Add(new Release
                    {
                        Id = Guid.NewGuid(),
                        EntityId = author.Id,
                        ExternalId = item.GetProperty("id").GetString() ?? Guid.NewGuid().ToString(),
                        Title = info.GetProperty("title").GetString() ?? "Untitled",
                        Type = "Book",
                        Url = info.TryGetProperty("infoLink", out var link) ? link.GetString() : null,
                        CoverImageUrl = info.TryGetProperty("imageLinks", out var img)
                            ? img.GetProperty("thumbnail").GetString()
                            : null,
                        Description = info.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                        ReleaseDate = pubDate
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Google Books error: {ex.Message}");
            }

            return releases;
        }

        public async Task<IEnumerable<Release>> GetOpenLibraryReleasesAsync(Entity author)
        {
            var releases = new List<Release>();
            if (string.IsNullOrWhiteSpace(author.ExternalId) || !author.ExternalId.Contains("/authors/"))
                return releases;

            var client = _httpClientFactory.CreateClient();
            var url = $"https://openlibrary.org{author.ExternalId}/works.json?limit=20";

            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return releases;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("entries", out var entries)) return releases;

                foreach (var entry in entries.EnumerateArray())
                {
                    var externalId = entry.GetProperty("key").GetString() ?? Guid.NewGuid().ToString();

                    DateTime? pubDate = null;
                    if (entry.TryGetProperty("first_publish_date", out var pubDateProp))
                        pubDate = ParseDate(pubDateProp.GetString());

                    releases.Add(new Release
                    {
                        Id = Guid.NewGuid(),
                        EntityId = author.Id,
                        ExternalId = externalId,
                        Title = entry.GetProperty("title").GetString() ?? "Untitled",
                        Type = "Book",
                        Url = $"https://openlibrary.org{externalId}",
                        ReleaseDate = pubDate,
                        Description = entry.TryGetProperty("subtitle", out var subtitle) ? subtitle.GetString() : null
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ OpenLibrary error: {ex.Message}");
            }

            return releases;
        }

        private DateTime? ParseDate(string? dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return null;

            if (DateTime.TryParse(dateStr, out var fullDate))
                return fullDate;

            if (DateTime.TryParseExact(dateStr, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var yearOnly))
                return new DateTime(yearOnly.Year, 1, 1);

            return null;
        }
    }
}
