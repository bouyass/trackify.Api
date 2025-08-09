namespace Trackify.Api.DataProviders.EntityProviders.Interfaces
{
    public interface IExternalGameCompanyEntityProvider
    {
        Task<string> RawgCompanyProvider(string query, int page, int pageSize);
        Task<string> IgdbCompanyProvider(string query, int page, int pageSize);
    }

}
