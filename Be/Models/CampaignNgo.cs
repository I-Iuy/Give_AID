namespace Be.Models
{
    public class CampaignNgo
    {
        public int CampaignId { get; set; }
        public int NgoId { get; set; }
        public Campaign Campaign { get; set; }
    }
}
