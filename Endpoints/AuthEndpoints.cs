using BCrypt.Net;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Trackify.Api.Data;
using Trackify.Api.Dtos;
using Trackify.Api.Models;
using Trackify.Api.Services;

namespace Trackify.Api.Endpoints
{
    public static class AuthEndpoints
    {
        private static class LogEvents
        {
            public static readonly EventId Ping = new(1000, nameof(Ping));
            public static readonly EventId GoogleSignIn = new(1001, nameof(GoogleSignIn));
            public static readonly EventId Register = new(1002, nameof(Register));
            public static readonly EventId Login = new(1003, nameof(Login));
            public static readonly EventId Refresh = new(1004, nameof(Refresh));
            public static readonly EventId Me = new(1005, nameof(Me));
            public static readonly EventId Logout = new(1006, nameof(Logout));
            public static readonly EventId LogoutAll = new(1007, nameof(LogoutAll));
            public static readonly EventId ExternalValidation = new(1010, nameof(ExternalValidation));
            public static readonly EventId DbSave = new(1020, nameof(DbSave));
        }


        private sealed class AuthLogCategory { }

        public static void MapAuthEndpoints(this WebApplication app)
        {
            string GenerateRefreshToken() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            static UserDto ToUserDto(User user) =>
                new()
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    PictureUrl = user.PictureUrl,
                    Provider = user.Provider,
                    Locale = user.Locale
                };

            static AuthResponseDto CreateAuthResponse(User user, RefreshToken refreshToken, IJwtService jwt) =>
                new()
                {
                    AccessToken = jwt.GenerateToken(user, refreshToken.Id),
                    RefreshToken = refreshToken.Token,
                    SessionId = refreshToken.Id,
                    User = ToUserDto(user)
                };

            app.MapGet("/auth/ping", (HttpContext http, ILogger<AuthLogCategory> logger) =>
            {
                logger.LogInformation(LogEvents.Ping, "Ping received. TraceId={TraceId}", http.TraceIdentifier);
                return Results.Ok("Auth endpoint works!");
            });

            app.MapPost("/auth/google", async (
                AuthDto dto,
                AppDbContext context,
                IJwtService jwt,
                HttpContext http,
                ILogger<AuthLogCategory> logger) =>
            {
                logger.LogInformation(LogEvents.GoogleSignIn,
                    "Google sign-in attempt. TraceId={TraceId}", http.TraceIdentifier);

                GoogleJsonWebSignature.Payload? payload = null;
                try
                {
                    payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken);
                    logger.LogInformation(LogEvents.ExternalValidation,
                        "Google token validated. Email={Email}, EmailVerified={EmailVerified}, TraceId={TraceId}",
                        payload.Email, payload.EmailVerified, http.TraceIdentifier);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(LogEvents.ExternalValidation,
                        ex, "Google token validation failed. TraceId={TraceId}", http.TraceIdentifier);
                    return Results.BadRequest(new ErrorResponseDto
                    {
                        Code = 1000,
                        Message = "Invalid Google token"
                    });
                }

                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

                var isNewUser = false;
                if (user == null)
                {
                    isNewUser = true;
                    user = new User
                    {
                        Email = payload.Email,
                        Username = payload.Name,
                        Provider = "google",
                        PictureUrl = payload.Picture,
                        Locale = payload.Locale
                    };
                    context.Users.Add(user);
                    logger.LogInformation(LogEvents.GoogleSignIn,
                        "Creating new user from Google. Email={Email}, TraceId={TraceId}",
                        user.Email, http.TraceIdentifier);
                }

                var refreshToken = new RefreshToken
                {
                    Token = GenerateRefreshToken(),
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    User = user
                };

                context.RefreshTokens.Add(refreshToken);

                try
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(LogEvents.DbSave,
                        "Saved {EntityCount} entities. NewUser={IsNewUser}, UserId={UserId}, SessionId={SessionId}, TraceId={TraceId}",
                        context.ChangeTracker.Entries().Count(),
                        isNewUser, user.Id, refreshToken.Id, http.TraceIdentifier);
                }
                catch (Exception ex)
                {
                    logger.LogError(LogEvents.DbSave, ex,
                        "Failed to save Google sign-in changes. UserEmail={Email}, TraceId={TraceId}",
                        user.Email, http.TraceIdentifier);
                    return Results.Problem("Unable to complete sign-in.");
                }

                return Results.Ok(CreateAuthResponse(user, refreshToken, jwt));
            })
            .Produces<AuthResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest);

            app.MapPost("/auth/register", async (
                RegisterDto dto,
                AppDbContext context,
                IJwtService jwt,
                HttpContext http,
                ILogger<AuthLogCategory> logger) =>
            {
                logger.LogInformation(LogEvents.Register,
                    "Register attempt. Email={Email}, Username={Username}, TraceId={TraceId}",
                    dto.Email, dto.Username, http.TraceIdentifier);

                if (await context.Users.AnyAsync(u => u.Email == dto.Email || u.Username == dto.Username))
                {
                    logger.LogWarning(LogEvents.Register,
                        "Register rejected: duplicate email/username. Email={Email}, Username={Username}, TraceId={TraceId}",
                        dto.Email, dto.Username, http.TraceIdentifier);

                    return Results.BadRequest(new ErrorResponseDto
                    {
                        Code = 1002,
                        Message = "Email or username already exists"
                    });
                }

                var user = new User
                {
                    Email = dto.Email,
                    Username = dto.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Provider = "local"
                };

                context.Users.Add(user);

                RefreshToken refreshToken;

                try
                {
                    refreshToken = new RefreshToken
                    {
                        Token = GenerateRefreshToken(),
                        ExpiresAt = DateTime.UtcNow.AddDays(7),
                        User = user
                    };

                    context.RefreshTokens.Add(refreshToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(LogEvents.DbSave, ex,
                        "Failed to generate token.", http.TraceIdentifier);
                    return Results.Problem("Unable to complete registration.");
                }               

                try
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(LogEvents.DbSave,
                        "User registered. UserId={UserId}, SessionId={SessionId}, TraceId={TraceId}",
                        user.Id, refreshToken.Id, http.TraceIdentifier);
                }
                catch (Exception ex)
                {
                    logger.LogError(LogEvents.DbSave, ex,
                        "Failed to save registration. Email={Email}, TraceId={TraceId}",
                        user.Email, http.TraceIdentifier);
                    return Results.Problem("Unable to complete registration.");
                }

                return Results.Ok(CreateAuthResponse(user, refreshToken, jwt));
            })
            .Produces<AuthResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest);

            app.MapPost("/auth/login", async (
                LoginDto dto,
                AppDbContext context,
                IJwtService jwt,
                HttpContext http,
                ILogger<AuthLogCategory> logger) =>
            {
                logger.LogInformation(LogEvents.Login,
                    "Login attempt. Identifier={Identifier}, TraceId={TraceId}",
                    dto.Identifier, http.TraceIdentifier);

                var user = await context.Users
                    .FirstOrDefaultAsync(u => u.Username == dto.Identifier || u.Email == dto.Identifier);

                if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                {
                    logger.LogWarning(LogEvents.Login,
                        "Invalid credentials. Identifier={Identifier}, TraceId={TraceId}",
                        dto.Identifier, http.TraceIdentifier);

                    return Results.BadRequest(new ErrorResponseDto
                    {
                        Code = 1001,
                        Message = "Invalid credentials"
                    });
                }

                var refreshToken = new RefreshToken
                {
                    Token = GenerateRefreshToken(),
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    User = user
                };

                context.RefreshTokens.Add(refreshToken);

                try
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(LogEvents.DbSave,
                        "User logged in. UserId={UserId}, SessionId={SessionId}, TraceId={TraceId}",
                        user.Id, refreshToken.Id, http.TraceIdentifier);
                }
                catch (Exception ex)
                {
                    logger.LogError(LogEvents.DbSave, ex,
                        "Failed to persist login. UserId={UserId}, TraceId={TraceId}",
                        user.Id, http.TraceIdentifier);
                    return Results.Problem("Unable to complete login.");
                }

                return Results.Ok(CreateAuthResponse(user, refreshToken, jwt));
            })
            .Produces<AuthResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest);

            app.MapPost("/auth/refresh", async (
               RefreshDto dto,
               AppDbContext context,
               IJwtService jwt,
               HttpContext http,
               ILogger<AuthLogCategory> logger) =>
            {
                logger.LogInformation(LogEvents.Refresh,
                    "Refresh attempt. TraceId={TraceId}", http.TraceIdentifier);

                var oldToken = await context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

                if (oldToken == null || oldToken.IsRevoked || oldToken.ExpiresAt < DateTime.UtcNow)
                {
                    logger.LogWarning(LogEvents.Refresh,
                        "Invalid/expired refresh token. Found={Found}, Revoked={Revoked}, Expired={Expired}, TraceId={TraceId}",
                        oldToken != null, oldToken?.IsRevoked, oldToken?.ExpiresAt < DateTime.UtcNow, http.TraceIdentifier);

                    return Results.BadRequest(new ErrorResponseDto
                    {
                        Code = 1003,
                        Message = "Invalid or expired refresh token"
                    });
                }

                oldToken.IsRevoked = true;

                var newToken = new RefreshToken
                {
                    Token = GenerateRefreshToken(),
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    User = oldToken.User
                };

                context.RefreshTokens.Add(newToken);

                try
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation(LogEvents.DbSave,
                        "Refreshed session. UserId={UserId}, OldSessionId={OldSessionId}, NewSessionId={NewSessionId}, TraceId={TraceId}",
                        oldToken.User.Id, oldToken.Id, newToken.Id, http.TraceIdentifier);
                }
                catch (Exception ex)
                {
                    logger.LogError(LogEvents.DbSave, ex,
                        "Failed to persist refresh. UserId={UserId}, TraceId={TraceId}",
                        oldToken.User.Id, http.TraceIdentifier);
                    return Results.Problem("Unable to refresh session.");
                }

                return Results.Ok(CreateAuthResponse(oldToken.User, newToken, jwt));
            })
            .Produces<AuthResponseDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest);

            app.MapGet("/auth/me", async (
                HttpContext http,
                AppDbContext context,
                ILogger<AuthLogCategory> logger) =>
            {
                var userIdClaim = http.User.FindFirst("sub")?.Value;
                if (userIdClaim == null)
                {
                    logger.LogWarning(LogEvents.Me,
                        "Unauthorized /auth/me call. TraceId={TraceId}", http.TraceIdentifier);
                    return Results.Unauthorized();
                }

                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    logger.LogWarning(LogEvents.Me,
                        "Invalid user id claim. Sub={Sub}, TraceId={TraceId}", userIdClaim, http.TraceIdentifier);
                    return Results.Unauthorized();
                }

                var user = await context.Users.FindAsync(userId);
                if (user == null)
                {
                    logger.LogWarning(LogEvents.Me,
                        "User not found. UserId={UserId}, TraceId={TraceId}", userId, http.TraceIdentifier);
                    return Results.NotFound();
                }

                logger.LogDebug(LogEvents.Me,
                    "/auth/me resolved. UserId={UserId}, TraceId={TraceId}", userId, http.TraceIdentifier);

                return Results.Ok(ToUserDto(user));
            })
            .RequireAuthorization()
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

            app.MapPost("/auth/logout", async (
                LogoutDto dto,
                AppDbContext context,
                HttpContext http,
                ILogger<AuthLogCategory> logger) =>
            {
                var token = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);
                if (token != null)
                {
                    token.IsRevoked = true;
                    await context.SaveChangesAsync();
                    logger.LogInformation(LogEvents.Logout,
                        "Logged out session. SessionId={SessionId}, TraceId={TraceId}",
                        token.Id, http.TraceIdentifier);
                }
                else
                {
                    logger.LogWarning(LogEvents.Logout,
                        "Logout called with unknown refresh token. TraceId={TraceId}",
                        http.TraceIdentifier);
                }
                return Results.Ok();
            })
              .RequireAuthorization();

            app.MapPost("/auth/logout-all", async (
                HttpContext http,
                AppDbContext context,
                ILogger<AuthLogCategory> logger) =>
            {
                var userIdClaim = http.User.FindFirst("sub")?.Value;
                if (userIdClaim == null)
                {
                    logger.LogWarning(LogEvents.LogoutAll,
                        "Unauthorized /auth/logout-all call. TraceId={TraceId}", http.TraceIdentifier);
                    return Results.Unauthorized();
                }

                var userId = Guid.Parse(userIdClaim);

                var tokens = await context.RefreshTokens
                    .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                    .ToListAsync();

                foreach (var t in tokens) t.IsRevoked = true;
                await context.SaveChangesAsync();

                logger.LogInformation(LogEvents.LogoutAll,
                    "Revoked all sessions. UserId={UserId}, RevokedCount={Count}, TraceId={TraceId}",
                    userId, tokens.Count, http.TraceIdentifier);

                return Results.Ok();
            })
                .RequireAuthorization();

            app.MapPost("/auth/reset-password", async (
            ResetPasswordDto dto,
            HttpContext http,
            AppDbContext context,
            ILogger<AuthLogCategory> logger) =>
            {
                var userIdClaim = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    logger.LogWarning("Unauthorized reset-password call. TraceId={TraceId}", http.TraceIdentifier);
                    return Results.BadRequest(new ErrorResponseDto
                    {
                        Code = 4002,
                        Details = "401",
                        Message = "Unauthorized reset-password call."
                    });
                }

                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    logger.LogWarning("Invalid user id claim for reset-password. TraceId={TraceId}", http.TraceIdentifier);
                    return Results.BadRequest(new ErrorResponseDto
                    {
                        Code = 4001,
                        Details = "401",
                        Message = "Invalid user id claim for reset-password"
                    });
                }

                var user = await context.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    logger.LogWarning("User not found for reset-password. TraceId={TraceId}", http.TraceIdentifier);
                    return Results.BadRequest( new ErrorResponseDto
                    {
                        Code = 4004,
                        Details = "401",
                        Message = "User not found for reset-password"
                    });
                }

                // Validate current password
                if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                {
                    return Results.BadRequest(new ErrorResponseDto
                    {
                        Code = 4003,
                        Details = "401",
                        Message = "Current password is incorrect"
                    });
                }


                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);


                foreach (var token in user.RefreshTokens)
                {
                    token.IsRevoked = true;
                }

                await context.SaveChangesAsync();

                logger.LogInformation("Password reset successful. UserId={UserId}, TraceId={TraceId}", user.Id, http.TraceIdentifier);

                return Results.Ok(new { Message = "Password reset successful" });
            })
                .RequireAuthorization()
                .Produces(StatusCodes.Status200OK)
                .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest);


            app.MapDelete("/auth/delete-account", async (
            [FromBody] DeleteAccountDto dto,
            HttpContext http,
            AppDbContext context,
            ILogger<AuthLogCategory> logger) =>
            {
                var userIdClaim = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    logger.LogWarning(LogEvents.LogoutAll, "Unauthorized delete-account call. TraceId={TraceId}", http.TraceIdentifier);
                    return Results.Unauthorized();
                }

                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    logger.LogWarning(LogEvents.LogoutAll, "Invalid user id claim for delete-account. TraceId={TraceId}", http.TraceIdentifier);
                    return Results.Unauthorized();
                }

                // Load user + tokens (and include other navigations you’ll need to clean)
                var user = await context.Users
                    .Include(u => u.RefreshTokens)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    logger.LogWarning(LogEvents.LogoutAll, "User not found for delete-account. UserId={UserId}, TraceId={TraceId}", userId, http.TraceIdentifier);
                    return Results.NotFound();
                }

                // Identity confirmation per provider
                if (user.Provider == "local")
                {
                    if (string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
                        !BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                    {
                        return Results.BadRequest(new ErrorResponseDto
                        {
                            Code = 2101,
                            Message = "Current password is incorrect"
                        });
                    }
                }
                else if (user.Provider == "google")
                {
                    if (string.IsNullOrWhiteSpace(dto.GoogleIdToken))
                    {
                        return Results.BadRequest(new ErrorResponseDto
                        {
                            Code = 2102,
                            Message = "Google ID token is required"
                        });
                    }

                    try
                    {
                        var payload = await GoogleJsonWebSignature.ValidateAsync(dto.GoogleIdToken);
                        // Match on email (and you can also check payload.Subject if you store it)
                        if (!string.Equals(payload.Email, user.Email, StringComparison.OrdinalIgnoreCase))
                        {
                            return Results.BadRequest(new ErrorResponseDto
                            {
                                Code = 2103,
                                Message = "Google token does not match account"
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(LogEvents.ExternalValidation, ex, "Google token validation failed for delete-account. TraceId={TraceId}", http.TraceIdentifier);
                        return Results.BadRequest(new ErrorResponseDto
                        {
                            Code = 2104,
                            Message = "Invalid Google token"
                        });
                    }
                }
                else
                {
                    // Unknown/unsupported provider
                    return Results.BadRequest(new ErrorResponseDto
                    {
                        Code = 2199,
                        Message = "Unsupported provider"
                    });
                }

                // ⚠️ Destructive: wrap in a transaction to avoid partial state
                using var tx = await context.Database.BeginTransactionAsync();
                try
                {
                    // Get the TrackifyUser record for this User
                    var trackifyUser = await context.TrackifyUsers
                        .FirstOrDefaultAsync(tu => tu.UserId == userId);

                    if (trackifyUser != null)
                    {
                        // Delete related UserUpdates
                        var updates = await context.UserUpdates
                            .Where(u => u.TrackifyUserId == trackifyUser.Id)
                            .ToListAsync();
                        context.UserUpdates.RemoveRange(updates);

                        // Delete related UserPreferenceLinks
                        var links = await context.UserPreferenceLinks
                            .Where(l => l.TrackifyUserId == trackifyUser.Id)
                            .ToListAsync();
                        context.UserPreferenceLinks.RemoveRange(links);

                        // Delete TrackifyUser itself
                        context.TrackifyUsers.Remove(trackifyUser);
                    }

                    // Revoke/delete refresh tokens
                    context.RefreshTokens.RemoveRange(user.RefreshTokens);

                    // Finally, delete the User
                    context.Users.Remove(user);

                    await context.SaveChangesAsync();
                    await tx.CommitAsync();

                    logger.LogInformation("Deleted account and related data for UserId={UserId}", userId);
                    return Results.Ok(new { message = "Account deleted successfully" });
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    logger.LogError(ex, "Failed to delete account. UserId={UserId}, TraceId={TraceId}", userId, http.TraceIdentifier);
                    return Results.Problem("Unable to delete account.");
                }
            })
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest);
        }
    }
}
