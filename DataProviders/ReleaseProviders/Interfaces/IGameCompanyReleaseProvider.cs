using Trackify.Api.Models;

namespace Trackify.Api.DataProviders.ReleaseProviders.Interfaces
{
    public interface IGameCompanyReleaseProvider
    {
        Task<IEnumerable<Release>> GetRawgReleasesAsync(Entity entity);
        Task<IEnumerable<Release>> GetIGDBReleasesAsync(Entity entity);
    }
}
