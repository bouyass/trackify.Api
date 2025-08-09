using Trackify.Api.Models;

public interface IGameCompanyMapper
{
    IEnumerable<Entity> MapRawg(string json);
    IEnumerable<Entity> MapIgdb(string json);
}
