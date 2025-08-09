using Microsoft.EntityFrameworkCore;
using Trackify.Api.Data;
using Trackify.Api.Dtos;
using Trackify.Api.Models;

namespace Trackify.Api.Endpoints
{
    public static class PreferencesEndpoints
    {
        public static void MapPreferencesEndpoints(this WebApplication app)
        {
            
            app.MapGet("/api/user/preferences", async (
                AppDbContext db,
                HttpContext http
            ) =>
            {
                var userIdClaim = http.User.FindFirst("id")?.Value;
                if (userIdClaim == null)
                    return Results.Unauthorized();

                var userId = Guid.Parse(userIdClaim);

                var userPreferences = await db.UserPreferenceLinks.Where(up => up.TrackifyUserId == userId)
                    .Select(up => up.PreferenceId)
                    .ToListAsync();

                var prefs = await db.Preferences
                    .Where(up => userPreferences.Contains(up.Id))
                    .Include(up => up.Entity.Releases)
                        .ThenInclude(r => r.Entity)
                            .ThenInclude(e => e.Category)
                    .ToListAsync();

                var preferences = await db.Preferences
                    .Where(up => userPreferences.Contains(up.Id))
                    .ToListAsync();
                return Results.Ok(preferences);
            });

            
            app.MapPost("/api/user/preferences", async (
                SavePreferenceDto dto,
                AppDbContext db,
                HttpContext http
            ) =>
            {
                var userIdClaim = http.User.FindFirst("id")?.Value;
                if (userIdClaim == null)
                    return Results.Unauthorized();

                var userId = Guid.Parse(userIdClaim);


                // Find or create Entity
                var entity = await db.Entities.FirstOrDefaultAsync(e => e.ExternalId == dto.ExternalId && e.Type == dto.Type);
                if (entity == null)
                {
                    entity = new Entity
                    {
                        ExternalId = dto.ExternalId,
                        Type = dto.Type,
                        Name = dto.Title,
                        Description = dto.Description,
                        AuthorOrArtist = dto.Author ?? dto.Artist,
                        Category = dto.Category
                    };
                    db.Entities.Add(entity);
                    await db.SaveChangesAsync();
                }

                if(entity == null)
                    return Results.BadRequest("Entity not found or created");

                var pref = await db.Preferences
                    .Include(p => p.Entity)
                    .FirstOrDefaultAsync(p => p.Entity.ExternalId == entity.ExternalId && p.Entity.Type == entity.Type);

                if (pref == null)
                {
                    pref = new Preference
                    {
                        EntityId = entity.Id,
                        CreatedAt = DateTime.UtcNow,
                        Entity = entity,
                        Id = Guid.NewGuid(),
                        LinkedUsers = new List<UserPreferenceLink>()
                    };
                }


                var userPref = await db.UserPreferenceLinks
                    .FirstOrDefaultAsync(up => up.TrackifyUserId == userId && up.PreferenceId == pref.Id);

                if (userPref == null)
                {
                    userPref = new UserPreferenceLink
                    {
                        Id = Guid.NewGuid(),
                        TrackifyUserId = userId,
                        PreferenceId = pref.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    db.UserPreferenceLinks.Add(userPref);
                    db.Preferences.Add(pref);
                }

                await db.SaveChangesAsync();

                return Results.Ok();
            });

        }
    }
}
