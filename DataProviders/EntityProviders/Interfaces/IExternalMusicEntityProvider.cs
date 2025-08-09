namespace Trackify.Api.DataProviders.EntityProviders.Interfaces
{
    public interface IExternalMusicEntityProvider
    {
        Task<string> SpotifyMusicProvider(string query, int page, int pageSize);
        Task<string> MusicBrainzProvider(string query, int page, int pageSize);
    }
}
