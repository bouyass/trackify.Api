using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trackify.Api.Data;
using Trackify.Api.Dtos;
using Trackify.Api.Models;
using Trackify.Api.Services;
using BCrypt.Net;

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
            });

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
            });
        }
    }
}
