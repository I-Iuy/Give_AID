using System.ComponentModel.DataAnnotations;

namespace Be.Models
{
    public class Account
    {
        [Key]
        public int AccountId { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string? FullName { get; set; }
        public string? DisplayName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        [Required]
        public string Role { get; set; } // "Admin", "User"

        public bool IsActive { get; set; } = true;

        //reset password
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }

    }

}
