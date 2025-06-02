using System.ComponentModel.DataAnnotations;

namespace Fe.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
