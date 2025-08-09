using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Trackify.Api.Models;
using Trackify.Api.Services; // ou le namespace où se trouvent tes services

public static class DataEndpoints
{
    public static void MapDataEndpoints(this WebApplication app)
    {
        app.MapGet("/api/data/authors", async (
            [FromServices] BooksSearchService booksService,
            [FromQuery] string? query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var result = await booksService.SearchAllAsync(query ?? "", page, pageSize);
            return Results.Ok(result);
        });

        app.MapGet("/api/data/artists", async (
            [FromServices] MusicSearchService musicService,
            [FromQuery] string? query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var result = await musicService.SearchAllAsync(query ?? "", page, pageSize);
            return Results.Ok(result);
        });

        app.MapGet("/api/data/studios", async (
            [FromServices] GameCompanySearchService gameStudioService,
            [FromQuery] string? query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var result = await gameStudioService.SearchAllAsync(query ?? "", page, pageSize);
            return Results.Ok(result);
        });
    }
}
