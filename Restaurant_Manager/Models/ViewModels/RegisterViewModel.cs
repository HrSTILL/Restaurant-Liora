using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class RegisterViewModel
{
    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    [RegularExpression(@"^[A-Za-zА-Яа-я]+$", ErrorMessage = "First name must contain only letters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    [RegularExpression(@"^[A-Za-zА-Яа-я]+$", ErrorMessage = "Last name must contain only letters.")]
    public string LastName { get; set; } = string.Empty;

    [Required, StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be exactly 10 digits.")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Phone must start with 0 and contain only digits.")]
    public string Phone { get; set; } = string.Empty;

    [Required, MinLength(6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
