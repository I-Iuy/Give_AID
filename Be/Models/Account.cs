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

        public string FullName { get; set; }
        public string DisplayName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        public string Name { get; set; }

        [Required]
        public string Role { get; set; } // "Admin", "User"

        public bool IsActive { get; set; } = true;


        // Navigation
        public ICollection<ContentPage>? ContentPages { get; set; }
    }

}
