using System.Text.Json;
using Trackify.Api.DataProviders.EntityProviders.Interfaces;
using Trackify.Api.Models;
using static Google.Apis.Requests.BatchRequest;

namespace Trackify.Api.DataProviders.EntityProviders
{
    public class BooksEntityProvider : IExternalBookEntityProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public BooksEntityProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<string> GoogleBooksProvider(string query, int page, int pageSize)
        {
            var client = _httpClientFactory.CreateClient();
            var startIndex = (page - 1) * pageSize;
            var googleBooksUrl = $"https://www.googleapis.com/books/v1/volumes?q=inauthor:{Uri.EscapeDataString(query)}&startIndex={startIndex}&maxResults={pageSize}";

            // Google Books
            try
            {
                var googleResponse = await client.GetAsync(googleBooksUrl);
                if (googleResponse.IsSuccessStatusCode)
                {
                    var json = await googleResponse.Content.ReadAsStringAsync();

                    return json;

                }
            }
            catch (Exception ex)
            {
                // Log exception or handle it accordingly
                Console.WriteLine($"Error fetching data from Google Books: {ex.Message}");
                return string.Empty;
            }
            return string.Empty;
        }


        public async Task<string> OpenLibraryBooksProvider(string query, int page, int pageSize)
        {
            var client = _httpClientFactory.CreateClient();
            var offset = (page - 1) * pageSize;
            var openLibraryUrl = $"https://openlibrary.org/search/authors.json?q={Uri.EscapeDataString(query)}&limit={pageSize}&offset={offset}";

            // Google Books
            try
            {
                var openLibraryResponse = await client.GetAsync(openLibraryUrl);
                if (openLibraryResponse.IsSuccessStatusCode)
                {
                    var json = await openLibraryResponse.Content.ReadAsStringAsync();

                    return json;

                }
            }
            catch (Exception ex)
            {
                // Log exception or handle it accordingly
                Console.WriteLine($"Error fetching data from Google Books: {ex.Message}");
                return string.Empty;
            }
            return string.Empty;
        }
    }
}
