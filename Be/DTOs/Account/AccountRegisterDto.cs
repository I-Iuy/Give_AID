using System.ComponentModel.DataAnnotations;

public class AccountRegisterDto
{
    [Required, EmailAddress]
    public string Email { get; set; }

    [Required, MinLength(1)]
    public string FullName { get; set; }

    [Required, MinLength(1)]
    public string DisplayName { get; set; }

    [Required, RegularExpression(@"^\+?[0-9]{7,15}$", ErrorMessage = "Invalid phone number format.")]
    public string Phone { get; set; }

    [Required, MinLength(1)]
    public string Address { get; set; }

    [Required, MinLength(6)]
    public string Password { get; set; }
}
