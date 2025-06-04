namespace Be.DTOs.Donations
{
    public class CreateDonationDto
    {
        public float Amount { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }

        public int? AccountId { get; set; } 

        public string? FullName { get; set; } 
        public string? Email { get; set; }
        public string? Address { get; set; }

        public int PurposeId { get; set; }
        public int? CampaignId { get; set; }
    }
}
