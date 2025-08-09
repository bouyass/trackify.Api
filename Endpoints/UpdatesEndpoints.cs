using Microsoft.EntityFrameworkCore;
using Trackify.Api.Data;

namespace Trackify.Api.Endpoints
{
    public static class UpdatesEndpoints
    {
        public static void MapUpdatesEndpoints(this WebApplication app)
        {
            app.MapGet("/api/user/updates", async (
                AppDbContext db,
                HttpContext http
            ) =>
            {
                var userIdClaim = http.User.FindFirst("id")?.Value;
                if (userIdClaim == null)
                    return Results.Unauthorized();

                var userId = Guid.Parse(userIdClaim);

                // Récupérer tous les UserUpdates avec Release + Entity + Category
                var updates = await db.UserUpdates
                    .Where(uu => uu.TrackifyUserId == userId)
                    .Include(uu => uu.Release)
                        .ThenInclude(r => r.Entity)
                            .ThenInclude(e => e.Category)
                    .ToListAsync();

                var result = updates.Select(uu => new
                {
                    UpdateId = uu.Id,
                    ReleaseId = uu.ReleaseId,
                    EntityId = uu.Release.Entity.Id,
                    Title = uu.Release.Entity.Name,
                    Category = uu.Release.Entity.Category,
                    Description = uu.Release.Entity.Description,
                    ReleaseDate = uu.Release.ReleaseDate,
                    UpdateDate = uu.CreatedAt,
                    IsRead = uu.IsRead,
                    ReadAt = uu.ReadAt
                });

                return Results.Ok(result);
            });
        }
    }
}
