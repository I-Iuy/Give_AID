namespace Be.DTOs.Partners
{
    public class UpdatePartnerDto
    {
        public int PartnerId { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public string ContractFile { get; set; }
        public int AccountId { get; set; }
    }
}
