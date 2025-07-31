namespace Trackify.Api.Endpoints
{
    public static class UpdatesEndpoints
    {
        public static void MapUpdatesEndpoints(this WebApplication app)
        {
            app.MapGet("/updates/ping", () => "Updates endpoint works!");
        }
    }
}
