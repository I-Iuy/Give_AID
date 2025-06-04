using System;

namespace Fe.DTOs.Share
{
    public class ShareDto
    {
        public int ShareId { get; set; }
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; } = string.Empty;
        public int? AccountId { get; set; }
        public string? AccountName { get; set; }
        public string? GuestName { get; set; }
        public string? ReceiverEmail { get; set; }
        public string Platform { get; set; } = "Email";
        public DateTime SharedAt { get; set; }
    }
}