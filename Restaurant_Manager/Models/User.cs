using System;
using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; 

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty; 

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty; 

    [Required]
    public string Password { get; set; } = string.Empty; 

    [Required]
    public string Role { get; set; } = string.Empty; 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
