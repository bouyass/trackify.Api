namespace Trackify.Api.Models
{
    public class ExternalProvider
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Provider { get; set; } = string.Empty; 
        public string ProviderUserId { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public User? User { get; set; }
    }
}