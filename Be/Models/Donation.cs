using System.ComponentModel.DataAnnotations;

public class Donation
{
    [Key] 
    public int DonationId { get; set; } // PK, auto-increment

    public float Amount { get; set; } 
    public DateTime DonatedAt { get; set; } 
    public string Method { get; set; } 
    public string Status { get; set; } 

    public int? AccountId { get; set; } // FK, nullable

    public string? FullName { get; set; } // nullable
    public string? Email { get; set; } // nullable
    public string? Address { get; set; } // nullable

    public int PurposeId { get; set; } // FK 
    public int? CampaignId { get; set; } // FK 
}
