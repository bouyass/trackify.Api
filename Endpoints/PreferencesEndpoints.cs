namespace Trackify.Api.Endpoints
{
    public static class PreferencesEndpoints
    {
        public static void MapPreferencesEndpoints(this WebApplication app)
        {
            app.MapGet("/preferences/ping", () => "Preferences endpoint works!");
        }

    }
}
