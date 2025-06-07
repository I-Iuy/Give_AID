namespace Be.DTOs.Account
{
    public class StatusUpdateDto
    {
        public bool IsActive { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
