using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant_Manager.Models;
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

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string DurationType { get; set; } = "Standard";

        [Required]
        public string SeatingArea { get; set; } = "Indoor";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
