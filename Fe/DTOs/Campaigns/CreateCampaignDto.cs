namespace Fe.DTOs.Campaigns
{
    public class CreateCampaignDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string VideoUrl { get; set; }
        public DateTime EventDate { get; set; }
        public int PurposeId { get; set; }
        public int AccountId { get; set; }
        public List<int> NgoIds { get; set; }
        public List<int> PartnerIds { get; set; }
    }
}
