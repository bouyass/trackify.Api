namespace Trackify.Api.Services.Interfaces
{
    public interface ISpotifyAuthService : IAuthService
    {
        public Task<string?> GetSpotifyAccessTokenAsync();
    }
}
