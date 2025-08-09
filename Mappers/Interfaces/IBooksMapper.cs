using Trackify.Api.Models;

namespace Trackify.Api.Mappers.Interfaces
{
    public interface IBooksMapper
    {
        IEnumerable<Entity> MapGoogleBooks(string json);
        IEnumerable<Entity> MapOpenLibrary(string json);
    }

}
