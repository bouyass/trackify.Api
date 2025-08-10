namespace Trackify.Api.Dtos
{
    public class DeleteAccountDto
    {
        public string? CurrentPassword { get; set; }
        public string? GoogleIdToken { get; set; } 
        public bool HardDelete { get; set; } = true;
    }
}
