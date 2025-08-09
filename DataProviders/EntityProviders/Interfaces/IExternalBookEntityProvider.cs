using Trackify.Api.Models;

namespace Trackify.Api.DataProviders.EntityProviders.Interfaces
{
    public interface IExternalBookEntityProvider
    {
        Task<string> GoogleBooksProvider(string query, int page, int pageSize);
        Task<string> OpenLibraryBooksProvider(string query, int page, int pageSize);    
    }
}
