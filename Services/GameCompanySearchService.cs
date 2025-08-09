using Trackify.Api.DataProviders.EntityProviders.Interfaces;
using Trackify.Api.Models;
using Trackify.Api.Services.Interfaces;

public class GameCompanySearchService : IEntitySearchService
{
    private readonly IExternalGameCompanyEntityProvider _provider;
    private readonly IGameCompanyMapper _mapper;

    public GameCompanySearchService(IExternalGameCompanyEntityProvider provider, IGameCompanyMapper mapper)
    {
        _provider = provider;
        _mapper = mapper;
    }

    public async Task<IEnumerable<Entity>> SearchAllAsync(string query, int page, int pageSize)
    {
        var rawgJson = await _provider.RawgCompanyProvider(query, page, pageSize);
        var igdbJson = await _provider.IgdbCompanyProvider(query, page, pageSize);

        var rawgEntities = _mapper.MapRawg(rawgJson);
        var igdbEntities = _mapper.MapIgdb(igdbJson);

        return rawgEntities.Concat(igdbEntities)
                           .GroupBy(e => e.ExternalId)
                           .Select(g => g.First())
                           .ToList();
    }
}
