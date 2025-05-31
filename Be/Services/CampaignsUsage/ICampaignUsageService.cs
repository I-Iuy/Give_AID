namespace Be.Services.CampaignUsage
{
    public interface ICampaignUsageService
    {
        Task<bool> IsPurposeUsedAsync(int purposeId);
        Task<bool> IsPartnerUsedAsync(int partnerId);
        Task<bool> IsNgoUsedAsync(int ngoId);
    }

}
