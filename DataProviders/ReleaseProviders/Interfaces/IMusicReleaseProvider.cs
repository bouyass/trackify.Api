using Trackify.Api.Models;

namespace Trackify.Api.DataProviders.ReleaseProviders.Interfaces
{
    public interface IMusicReleaseProvider
    {
        Task<IEnumerable<Release>> GetSpotifyReleasesAsync(Entity entity);
        Task<IEnumerable<Release>> GetMusicBrainzReleasesAsync(Entity entity);
    }
}
