namespace Be.Repositories.CampaignsUsage
{
    public interface ICampaignUsageRepository
    {
        Task<bool> IsPurposeUsedAsync(int purposeId);
        Task<bool> IsPartnerUsedAsync(int partnerId);
        Task<bool> IsNgoUsedAsync(int ngoId);
    }

}
