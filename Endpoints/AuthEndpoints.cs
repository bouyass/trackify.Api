using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Trackify.Api.Data;
using Trackify.Api.Dtos;
using Trackify.Api.Models;
using Trackify.Api.Services;
using BCrypt.Net;

namespace Trackify.Api.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {

            string GenerateRefreshToken() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            static UserDto ToUserDto(User user) =>
                new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    PictureUrl = user.PictureUrl,
                    Provider = user.Provider,
                    Locale = user.Locale
                };

            static AuthResponseDto CreateAuthResponse(User user, RefreshToken refreshToken, IJwtService jwt) =>
                new AuthResponseDto
                {
                    AccessToken = jwt.GenerateToken(user, refreshToken.Id),
                    RefreshToken = refreshToken.Token,
                    SessionId = refreshToken.Id,
                    User = ToUserDto(user)
                };


            app.MapGet("/auth/ping", () => "Auth endpoint works!");

            app.MapPost("/auth/google", async (
                AuthDto dto,
                AppDbContext context,
                IJwtService jwt) =>
            {

                var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken);

                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

                if (user == null)
                {
                    user = new User
                    {
                        Email = payload.Email,
                        Username = payload.Name,
                        Provider = "google"
                    };
                    context.Users.Add(user);
                }

                var refreshToken = new RefreshToken
                {
                    Token = GenerateRefreshToken(),
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    User = user
                };

                context.RefreshTokens.Add(refreshToken);
                await context.SaveChangesAsync();

                return Results.Ok(CreateAuthResponse(user, refreshToken, jwt));

            });

            app.MapPost("/auth/register", async (
                RegisterDto dto,
                AppDbContext context,
                IJwtService jwt) =>
            {
                if (await context.Users.AnyAsync(u => u.Email == dto.Email || u.Username == dto.Username))
                    return Results.BadRequest(new { message = "Email or username already exists" });

                var user = new User
                {
                    Email = dto.Email,
                    Username = dto.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Provider = "local"
                };

                var refreshToken = new RefreshToken
                {
                    Token = GenerateRefreshToken(),
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    User = user
                };

                context.Users.Add(user);
                context.RefreshTokens.Add(refreshToken);
                await context.SaveChangesAsync();

                return Results.Ok(CreateAuthResponse(user, refreshToken, jwt));


            });

            app.MapPost("/auth/login", async (
                LoginDto dto,
                AppDbContext context,
                IJwtService jwt) =>
            {
                var user = await context.Users
                    .FirstOrDefaultAsync(u => u.Username == dto.Identifier || u.Email == dto.Identifier);

                if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                    return Results.BadRequest(new { message = "Invalid credentials" });

                var refreshToken = new RefreshToken
                {
                    Token = GenerateRefreshToken(),
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    User = user
                };

                context.RefreshTokens.Add(refreshToken);
                await context.SaveChangesAsync();

                return Results.Ok(CreateAuthResponse(user, refreshToken, jwt));
            });

            app.MapPost("/auth/refresh", async (
               RefreshDto dto,
               AppDbContext context,
               IJwtService jwt) =>
            {
                var oldToken = await context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

                if (oldToken == null || oldToken.IsRevoked || oldToken.ExpiresAt < DateTime.UtcNow)
                    return Results.BadRequest(new { message = "Invalid or expired refresh token" });

                oldToken.IsRevoked = true;

                var newToken = new RefreshToken
                {
                    Token = GenerateRefreshToken(),
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    User = oldToken.User
                };

                context.RefreshTokens.Add(newToken);

                await context.SaveChangesAsync();

                return Results.Ok(CreateAuthResponse(oldToken.User, newToken, jwt));
            });

            app.MapGet("/auth/me", async (
                HttpContext http,
                AppDbContext context) =>
            {
                var userIdClaim = http.User.FindFirst("sub")?.Value;
                if (userIdClaim == null) return Results.Unauthorized();


                var user = await context.Users.FindAsync(Guid.Parse(userIdClaim));
                if (user == null) return Results.NotFound();

                return Results.Ok(ToUserDto(user));
            }).RequireAuthorization();

            app.MapPost("/auth/logout", async (
                LogoutDto dto,
                AppDbContext context) =>
            {
                var token = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);
                if (token != null) token.IsRevoked = true;
                await context.SaveChangesAsync();
                return Results.Ok();
            });

            app.MapPost("/auth/logout-all", async (
                HttpContext http,
                AppDbContext context) => {

                var userIdClaim = http.User.FindFirst("sub")?.Value;
                if (userIdClaim == null) return Results.Unauthorized();

                var tokens = await context.RefreshTokens
                    .Where(rt => rt.UserId == Guid.Parse(userIdClaim) && !rt.IsRevoked)
                    .ToListAsync();

                foreach (var t in tokens) t.IsRevoked = true;
                await context.SaveChangesAsync();

                return Results.Ok();
            });
        }
    }
}
