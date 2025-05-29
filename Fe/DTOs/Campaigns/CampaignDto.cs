namespace Fe.DTOs.Campaigns
{
    public class CampaignDto
    {
        public int CampaignId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string VideoUrl { get; set; }
        public DateTime EventDate { get; set; }
        public string PurposeTitle { get; set; }
        public int AccountId { get; set; }
        public List<string> NgoNames { get; set; }
        public List<string> PartnerNames { get; set; }

    }

}
