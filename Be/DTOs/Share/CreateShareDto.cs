namespace Be.DTOs.Share
{
    public class CreateShareDto
    {
        public int CampaignId { get; set; }
        public int? AccountId { get; set; }         // Người đăng nhập
        public string? GuestName { get; set; }      // Nếu là người dùng chưa đăng nhập
        public string? ReceiverEmail { get; set; }  // Dùng khi chia sẻ qua Email
        public string Platform { get; set; } = "Email";  // Facebook, WhatsApp, v.v.
    }
}
