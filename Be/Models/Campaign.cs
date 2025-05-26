using System.ComponentModel.DataAnnotations;

namespace Be.Models
{
    public class Campaign
    {
        [Key]
        public int CampaignId { get; set; } // PK, tự tăng
        public string Title { get; set; }
        public string Content { get; set; }
        public string VideoUrl { get; set; }
        public DateTime EventDate { get; set; }
        public int AccountId { get; set; } // khóa ngoại
        public int PurposeId { get; set; } // khóa ngoại
        public ICollection<CampaignNgo> CampaignNgos { get; set; }
        public ICollection<CampaignPartner> CampaignPartners { get; set; }

    }
}
