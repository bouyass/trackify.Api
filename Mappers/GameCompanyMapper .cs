using System.Text.Json;
using Trackify.Api.Models;

public class GameCompanyMapper : IGameCompanyMapper
{
    public IEnumerable<Entity> MapRawg(string json)
    {
        var results = new List<Entity>();
        if (string.IsNullOrWhiteSpace(json)) return results;

        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("results", out var companies)) return results;

        foreach (var item in companies.EnumerateArray())
        {
            results.Add(new Entity
            {
                ExternalId = item.GetProperty("id").GetRawText(),
                Name = item.GetProperty("name").GetString() ?? "Unknown",
                Type = "Studio",
                Category = "VideoGame",
                Description = item.TryGetProperty("games_count", out var gamesCount)
                              ? $"Games: {gamesCount.GetInt32()}"
                              : null,
                ImageUrl = item.TryGetProperty("image_background", out var image) ? image.GetString() : null
            });
        }

        return results;
    }

    public IEnumerable<Entity> MapIgdb(string json)
    {
        var results = new List<Entity>();
        if (string.IsNullOrWhiteSpace(json)) return results;

        using var doc = JsonDocument.Parse(json);
        foreach (var item in doc.RootElement.EnumerateArray())
        {
            results.Add(new Entity
            {
                ExternalId = item.GetProperty("id").GetRawText(),
                Name = item.GetProperty("name").GetString() ?? "Unknown",
                Type = "Studio",
                Category = "VideoGame",
                Description = item.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                ImageUrl = item.TryGetProperty("logo", out var logo) ? $"https://images.igdb.com/igdb/image/upload/t_logo_med/{logo.GetRawText()}.png" : null
            });
        }

        return results;
    }
}
