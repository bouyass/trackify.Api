using Trackify.Api.DataProviders.EntityProviders.Interfaces;
using Trackify.Api.Mappers.Interfaces;
using Trackify.Api.Models;
using Trackify.Api.Services.Interfaces;

namespace Trackify.Api.Services
{
    public class BooksSearchService : IEntitySearchService
    {

        private readonly IExternalBookEntityProvider _booksProvider;
        private readonly IBooksMapper _mapper;

        public BooksSearchService(IExternalBookEntityProvider booksProvider, IBooksMapper mapper)
        {
            _booksProvider = booksProvider;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Entity>> SearchAllAsync(string query, int page, int pageSize)
        {
            var trimmedQuery = query?.Trim();

            if (string.IsNullOrWhiteSpace(trimmedQuery))
            {
                // Récupération par défaut (populaires ou aléatoires)
                var defaultJson = await _booksProvider.GoogleBooksProvider(trimmedQuery, page, pageSize);
                return _mapper.MapGoogleBooks(defaultJson);
            }

            var googleJson = await _booksProvider.GoogleBooksProvider(trimmedQuery, page, pageSize);
            var openLibraryJson = await _booksProvider.OpenLibraryBooksProvider(trimmedQuery, page, pageSize);

            var googleEntities = _mapper.MapGoogleBooks(googleJson);
            var openLibraryEntities = _mapper.MapOpenLibrary(openLibraryJson);

            return googleEntities.Concat(openLibraryEntities)
                                 .GroupBy(e => e.ExternalId)
                                 .Select(g => g.First())
                                 .ToList();
        }
    }
}
