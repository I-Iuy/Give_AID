namespace Be.DTOs.Share
{
    public class ShareDto
    {
        public int ShareId { get; set; }
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; } = string.Empty; // bổ sung hiển thị
        public int? AccountId { get; set; }
        public string? AccountName { get; set; }     // full_name hoặc display_name
        public string? GuestName { get; set; }
        public string? ReceiverEmail { get; set; }
        public string Platform { get; set; } = "Email";
        public DateTime SharedAt { get; set; }
    }
}
