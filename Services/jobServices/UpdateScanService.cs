using Microsoft.EntityFrameworkCore;
using Trackify.Api.Data;
using Trackify.Api.DataProviders.ReleaseProviders.Interfaces;
using Trackify.Api.Models;

namespace Trackify.Api.Services;

public class UpdateScanService
{
    private readonly AppDbContext _db;
    private readonly IBookReleaseProvider _bookReleaseProvider;

    public UpdateScanService(AppDbContext db, IBookReleaseProvider bookReleaseProvider)
    {
        _db = db;
        _bookReleaseProvider = bookReleaseProvider;
    }

    public async Task RunDailyAsync()
    {
        Console.WriteLine("📘 Début du scan des nouveautés (books)...");

        var authors = await _db.Entities
            .Include(e => e.Releases)
            .Where(e => e.Type == "Author")
            .ToListAsync();

        int totalNewReleases = 0;

        foreach (var author in authors)
        {
            var googleReleases = await _bookReleaseProvider.GetGoogleBooksReleasesAsync(author);
            var openLibReleases = await _bookReleaseProvider.GetOpenLibraryReleasesAsync(author);

            var allReleases = googleReleases.Concat(openLibReleases)
                .GroupBy(r => r.ExternalId)
                .Select(g => g.First())
                .ToList();

            foreach (var release in allReleases)
            {
                var alreadyExists = await _db.Releases
                    .AnyAsync(r => r.ExternalId == release.ExternalId && r.EntityId == release.EntityId);

                if (alreadyExists) continue;

                // Ajouter la release
                _db.Releases.Add(release);
                totalNewReleases++;

                // Trouver les utilisateurs qui suivent cette entité
                var userIds = await _db.UserPreferenceLinks
                 .Where(link => link.Preference.EntityId == release.EntityId)
                 .Select(link => link.TrackifyUserId)
                 .ToListAsync();

                foreach (var userId in userIds)
                {
                    var userUpdate = new UserUpdate
                    {
                        Id = Guid.NewGuid(),
                        TrackifyUserId = userId,
                        ReleaseId = release.Id,
                        Type = "NewRelease",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    _db.UserUpdates.Add(userUpdate);
                }

                Console.WriteLine($"✨ Nouvelle release trouvée: {release.Title} ({author.Name})");
            }
        }

        await _db.SaveChangesAsync();
        Console.WriteLine($"✅ Scan terminé. {totalNewReleases} nouvelles releases ajoutées.");
    }
}
