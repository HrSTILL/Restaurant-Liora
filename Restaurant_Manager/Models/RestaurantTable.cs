using System.ComponentModel.DataAnnotations;

public class RestaurantTable
{
    public int Id { get; set; }

    [Required]
    public int TableNumber { get; set; }

    [Required]
    public int Seats { get; set; }

    [Required]
    public string Area { get; set; } = "Indoor"; 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
