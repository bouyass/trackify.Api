using Trackify.Api.DataProviders.EntityProviders.Interfaces;
using Trackify.Api.Mappers.Interfaces;
using Trackify.Api.Models;
using Trackify.Api.Services.Interfaces;

public class MusicSearchService : IEntitySearchService
{
    private readonly IExternalMusicEntityProvider _musicProvider;
    private readonly IMusicMapper _mapper;

    public MusicSearchService(IExternalMusicEntityProvider musicProvider, IMusicMapper mapper)
    {
        _musicProvider = musicProvider;
        _mapper = mapper;
    }

    public async Task<IEnumerable<Entity>> SearchAllAsync(string query, int page, int pageSize)
    {
        var spotifyJson = await _musicProvider.SpotifyMusicProvider(query, page, pageSize);
        var musicBrainzJson = await _musicProvider.MusicBrainzProvider(query, page, pageSize);

        var spotifyEntities = _mapper.MapSpotify(spotifyJson);
        var musicBrainzEntities = _mapper.MapMusicBrainz(musicBrainzJson);

        return spotifyEntities.Concat(musicBrainzEntities)
                              .GroupBy(e => e.ExternalId)
                              .Select(g => g.First())
                              .ToList();
    }
}
