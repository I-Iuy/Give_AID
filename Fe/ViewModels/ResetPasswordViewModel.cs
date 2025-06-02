using System.ComponentModel.DataAnnotations;

namespace Fe.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; }

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required, Compare("NewPassword")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
