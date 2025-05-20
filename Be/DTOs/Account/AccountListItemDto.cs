namespace Be.DTOs.Account
{
    public class AccountListItemDto
    {
        public int AccountId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string DisplayName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
    }

}
