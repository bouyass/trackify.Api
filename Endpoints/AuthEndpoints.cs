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
            app.MapGet("/auth/ping", () => "Auth endpoint works!");

            string GenerateRefreshToken() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            app.MapPost("/auth/google", async (
                AuthDto dto,
                AppDbContext context,
                IJwtService jwt) =>
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken);

                var user = await context.Users
                    .Include(u => u.ExternalProviders)
                    .FirstOrDefaultAsync(u => u.ExternalProviders
                        .Any(p => p.Provider == "google" && p.ProviderUserId == payload.Subject));

                if (user == null)
                {
                    user = new User
                    {
                        Email = payload.Email,
                        Username = payload.Name,
                        PictureUrl = payload.Picture
                    };
                    context.Users.Add(user);
                    context.ExternalProviders.Add(new ExternalProvider
                    {
                        User = user,
                        Provider = "google",
                        ProviderUserId = payload.Subject
                    });
                    await context.SaveChangesAsync();
                }

                var token = jwt.GenerateToken(user);

                return Results.Ok(new { token });
            });

            app.MapPost("/auth/register", async (
                RegisterDto dto,
                AppDbContext context,
                IJwtService jwt) =>
            {

                if (await context.Users.AnyAsync(u => u.Email == dto.Email || u.Username == dto.Username))
                {
                    return Results.BadRequest(new { message = "Email or username already exists" });
                }

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                var user = new User
                {
                    Email = dto.Email,
                    Username = dto.Username,
                    PasswordHash = passwordHash
                };

                context.Users.Add(user);
                await context.SaveChangesAsync();

                var token = jwt.GenerateToken(user);
                return Results.Ok(new { token });
            });

            app.MapPost("/auth/login", async (
                LoginDto dto,
                AppDbContext context,
                IJwtService jwt) =>
            {
                // Find by username or email
                var user = await context.Users
                    .FirstOrDefaultAsync(u => u.Username == dto.Identifier || u.Email == dto.Identifier);

                if (user == null)
                    return Results.BadRequest(new { message = "Invalid credentials" });

                // Validate password
                if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                    return Results.BadRequest(new { message = "Invalid credentials" });

                var token = jwt.GenerateToken(user);
                return Results.Ok(new { token });
            });

            app.MapPost("/auth/refresh", async (
               RefreshDto dto,
               AppDbContext context,
               IJwtService jwt) =>
            {
                var refresh = await context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

                if (refresh == null || refresh.IsRevoked || refresh.ExpiresAt < DateTime.UtcNow)
                    return Results.BadRequest(new { message = "Invalid or expired refresh token" });

                // Revoke old token (rotation)
                refresh.IsRevoked = true;

                // Issue new refresh token
                var newRefreshToken = new RefreshToken
                {
                    Token = GenerateRefreshToken(),
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    User = refresh.User
                };
                context.RefreshTokens.Add(newRefreshToken);

                await context.SaveChangesAsync();

                var token = jwt.GenerateToken(refresh.User);
                return Results.Ok(new { token, refreshToken = newRefreshToken.Token });
            });
        }
    }
}
