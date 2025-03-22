using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty; 

    [Required, MinLength(6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
