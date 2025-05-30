namespace Fe.DTOs.Share
{
    public class CreateShareDto
    {
        public int CampaignId { get; set; }
        public int? AccountId { get; set; }
        public string? GuestName { get; set; }
        public string? ReceiverEmail { get; set; }
        public string Platform { get; set; } = "Email";
    }
}
