namespace Fe.DTOs.Donations
{
    public class DonationDto
    {
        public int DonationId { get; set; }
        public float Amount { get; set; }
        public DateTime DonatedAt { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }

        public int? AccountId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }

        public int PurposeId { get; set; }
        public int? CampaignId { get; set; }

        public string? PurposeTitle { get; set; }
        public string? CampaignTitle { get; set; }
    }
}
