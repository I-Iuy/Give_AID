using System.ComponentModel.DataAnnotations;

namespace Fe.ViewModels
{
    public class AccountRegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

        public string FullName { get; set; }
        public string DisplayName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public string Role { get; set; } = "User";
    }
}
