using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Reservation
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }  

    [Required]
    public int TableId { get; set; }

    [ForeignKey("TableId")]
    public RestaurantTable Table { get; set; }  

    [Required]
    public DateTime ReservationTime { get; set; }

    [Required]
    public string Status { get; set; } = "pending"; 

    [Required]
    public int NumberOfPeople { get; set; } 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
