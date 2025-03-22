using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty; 

    [Required]
    [Column(TypeName = "TEXT")]
    [RegularExpression("^(admin|staff|customer)$", ErrorMessage = "Invalid role.")]
    public string Role { get; set; } = "customer"; 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public void SetPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        PasswordHash = Convert.ToBase64String(hashedBytes);
    }

    public bool VerifyPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return PasswordHash == Convert.ToBase64String(hashedBytes);
    }
}
