using System.ComponentModel.DataAnnotations;

namespace Fe.ViewModels
{
    public class AccountLoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
