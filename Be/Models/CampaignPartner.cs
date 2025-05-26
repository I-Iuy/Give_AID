namespace Be.Models
{
    public class CampaignPartner
    {
        public int CampaignId { get; set; }
        public int PartnerId { get; set; }
        public Campaign Campaign { get; set; }
    }
}
