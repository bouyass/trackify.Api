using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trackify.Api.Data;
using Trackify.Api.Models;

public static class DataEndpoints
{
    public static void MapDataEndpoints(this WebApplication app)
    {
        app.MapGet("/api/data/authors", async (string query, [FromServices] IHttpClientFactory httpClientFactory) =>
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://www.googleapis.com/books/v1/volumes?q=inauthor:{Uri.EscapeDataString(query)}&maxResults=20");

            if (!response.IsSuccessStatusCode)
                return Results.Problem("Error fetching data from Google Books API");

            var json = await response.Content.ReadAsStringAsync();

            // Optionally parse and extract unique authors here or send raw data to frontend
            return Results.Content(json, "application/json");
        });

        app.MapGet("/api/data/artists", async (string query, [FromServices] IHttpClientFactory httpClientFactory) =>
        {
            var client = httpClientFactory.CreateClient();
            var url = $"https://itunes.apple.com/search?term={Uri.EscapeDataString(query)}&entity=musicArtist&limit=25";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return Results.Problem("Error fetching data from iTunes API");

            var json = await response.Content.ReadAsStringAsync();
            return Results.Content(json, "application/json");
        });


        app.MapGet("/api/data/studios", async (string query, [FromServices] IHttpClientFactory httpClientFactory) =>
        {
            var client = httpClientFactory.CreateClient();
            var apiKey = "YOUR_TMDB_API_KEY";

            // Search companies by name (studios)
            var url = $"https://api.themoviedb.org/3/search/company?api_key={apiKey}&query={Uri.EscapeDataString(query)}";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return Results.Problem("Error fetching data from TMDb API");

            var json = await response.Content.ReadAsStringAsync();
            return Results.Content(json, "application/json");
        });
    }
}