using System.ComponentModel.DataAnnotations;

namespace Be.Models
{
    public class Campaign
    {
        [Key]
        public int CampaignId { get; set; } // PK, auto-increment
        public string Title { get; set; }
        public string Content { get; set; }
        public string VideoUrl { get; set; }
        public DateTime EventDate { get; set; }
        public int AccountId { get; set; } // FK
        public int PurposeId { get; set; } // FK
        public ICollection<CampaignNgo> CampaignNgos { get; set; }
        public ICollection<CampaignPartner> CampaignPartners { get; set; }

    }
}
