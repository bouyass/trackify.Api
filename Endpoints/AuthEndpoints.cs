namespace Trackify.Api.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            // For now, just a placeholder endpoint
            app.MapGet("/auth/ping", () => "Auth endpoint works!");
        }
    }
}
