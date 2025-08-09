using Trackify.Api.Models;

namespace Trackify.Api.DataProviders.ReleaseProviders.Interfaces
{
    public interface IBookReleaseProvider
    {
        Task<IEnumerable<Release>> GetGoogleBooksReleasesAsync(Entity author);
        Task<IEnumerable<Release>> GetOpenLibraryReleasesAsync(Entity author);
    }
}
