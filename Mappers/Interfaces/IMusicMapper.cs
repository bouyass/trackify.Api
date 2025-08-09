using Trackify.Api.Models;

namespace Trackify.Api.Mappers.Interfaces
{
    public interface IMusicMapper
    {
        IEnumerable<Entity> MapSpotify(string json);
        IEnumerable<Entity> MapMusicBrainz(string json);
    }

}
