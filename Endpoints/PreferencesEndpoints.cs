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

                // Get all preferences for this user, including Release -> Entity -> Category
                var preferences = await db.UserPreferences
                    .Where(up => up.UserId == userId)
                    .Include(up => up.Entity.Releases)
                        .ThenInclude(r => r.Entity)
                            .ThenInclude(e => e.Category)
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

                // Check if UserPreference exists
                var existingPref = await db.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId && p.EntityId == entity.Id);
                if (existingPref != null)
                    return Results.Conflict("Preference already exists");

                // Add UserPreference
                var preference = new UserPreference
                {
                    UserId = userId,
                    EntityId = entity.Id
                };
                db.UserPreferences.Add(preference);
                await db.SaveChangesAsync();

                return Results.Ok();
            });

        }
    }
}
