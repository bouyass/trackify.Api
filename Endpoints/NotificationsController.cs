using Trackify.Api.Dtos;

namespace Trackify.Api.Endpoints
{
    public static class NotificationsEndpoints
    {
        public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/notifications/devices/register", async (
                RegisterDeviceDto dto,
                HttpClient http,
                IConfiguration config,
                ILoggerFactory loggerFactory) =>
            {
                var logger = loggerFactory.CreateLogger("Notifications");
                var notificationsUrl = config["Notifications:Url"] ?? config["notificationsServiceUrl"]
                    ?? throw new InvalidOperationException("Notifications:Url is not configured.");

                try
                {
                    var response = await http.PostAsJsonAsync($"{notificationsUrl}/devices", dto);

                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        logger.LogWarning("Failed to register device. Status={Status}, Error={Error}",
                            response.StatusCode, error);
                        return Results.StatusCode((int)response.StatusCode);
                    }

                    var result = await response.Content.ReadAsStringAsync();
                    return Results.Content(result, "application/json");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Exception while registering device");
                    return Results.Problem("Internal server error while registering device");
                }
            })
            .RequireAuthorization(); 


            return app;
        }
    }
}
