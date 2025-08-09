using System.Text.Json;
using Trackify.Api.Mappers.Interfaces;
using Trackify.Api.Models;

public class MusicMapper : IMusicMapper
{
    public IEnumerable<Entity> MapSpotify(string json)
    {
        var results = new List<Entity>();
        if (string.IsNullOrWhiteSpace(json)) return results;

        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("artists", out var artists) ||
            !artists.TryGetProperty("items", out var items))
            return results;

        foreach (var item in items.EnumerateArray())
        {
            results.Add(new Entity
            {
                ExternalId = item.GetProperty("id").GetString() ?? "",
                Name = item.GetProperty("name").GetString() ?? "Unknown",
                Type = "Artist",
                Category = "Music",
                Description = item.TryGetProperty("genres", out var genres) && genres.GetArrayLength() > 0
                              ? string.Join(", ", genres.EnumerateArray().Select(g => g.GetString()))
                              : null,
                ImageUrl = item.TryGetProperty("images", out var images) && images.GetArrayLength() > 0
                           ? images[0].GetProperty("url").GetString()
                           : null
            });
        }

        return results;
    }

    public IEnumerable<Entity> MapMusicBrainz(string json)
    {
        var results = new List<Entity>();
        if (string.IsNullOrWhiteSpace(json)) return results;

        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("artists", out var artists))
            return results;

        foreach (var item in artists.EnumerateArray())
        {
            results.Add(new Entity
            {
                ExternalId = item.GetProperty("id").GetString() ?? "",
                Name = item.GetProperty("name").GetString() ?? "Unknown",
                Type = "Artist",
                Category = "Music",
                Description = item.TryGetProperty("disambiguation", out var desc) ? desc.GetString() : null
            });
        }

        return results;
    }
}
