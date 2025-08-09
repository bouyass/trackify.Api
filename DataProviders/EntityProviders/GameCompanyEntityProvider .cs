using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Trackify.Api.DataProviders.EntityProviders.Interfaces;

namespace Trackify.Api.DataProviders.EntityProviders
{
    public class GameCompanyEntityProvider : IExternalGameCompanyEntityProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GameCompanyEntityProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> RawgCompanyProvider(string query, int page, int pageSize)
        {
            var client = _httpClientFactory.CreateClient();
            var rawgUrl = $"https://api.rawg.io/api/developers?key=4a160fd090134d8281040c28e2f79a35&search={Uri.EscapeDataString(query)}&page={page}&page_size={pageSize}";
            try
            {
                var response = await client.GetAsync(rawgUrl);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching RAWG data: {ex.Message}");
            }
            return string.Empty;
        }

        public async Task<string> IgdbCompanyProvider(string query, int page, int pageSize)
        {
            var offset = (page - 1) * pageSize;
            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Add("Client-ID", "YOUR_TWITCH_CLIENT_ID");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer YOUR_TWITCH_ACCESS_TOKEN");

            var url = "https://api.igdb.com/v4/companies";

            var body = string.IsNullOrWhiteSpace(query)
                ? $"fields id,name,description,logo; limit {pageSize}; offset {offset};"
                : $"search \"{query}\"; fields id,name,description,logo; limit {pageSize}; offset {offset};";

            var content = new StringContent(body, System.Text.Encoding.UTF8, "text/plain");

            try
            {
                var response = await client.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IGDB error: {ex.Message}");
            }

            return string.Empty;
        }
    }
}
