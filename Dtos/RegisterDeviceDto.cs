namespace Trackify.Api.Dtos
{
    public class RegisterDeviceDto
    {
        public string Platform { get; set; } = default!;
        public string Provider { get; set; } = default!;
        public string PushToken { get; set; } = default!;
        public string ExternalUserId { get; set; } = default!;
    }
}
