using System;
using System.ComponentModel.DataAnnotations;


namespace Restaurant_Manager.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public int Calories { get; set; }

        public string Allergens { get; set; }

        public string Tags { get; set; }
        [Required]
        public bool IsGlutenFree { get; set; }
        [Required]
        public int PrepTimeMinutes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}