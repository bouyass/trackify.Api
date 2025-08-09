using Trackify.Api.Models;

namespace Trackify.Api.Services.Interfaces
{
    public interface IEntitySearchService
    {
        public Task<IEnumerable<Entity>> SearchAllAsync(string query, int page, int pageSize);
    }
}
