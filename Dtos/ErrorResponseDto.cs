namespace Trackify.Api.Dtos
{
    public class ErrorResponseDto
    {
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
    }
}
