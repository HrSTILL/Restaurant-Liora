using Restaurant_Manager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Restaurant_Manager.ViewModels
{
    public class CustomerReservationsViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Range(1, 12)]
        public int NumberOfPeople { get; set; }

        [Required]
        public DateTime ReservationDate { get; set; }

        [Required]
        public TimeSpan ReservationHour { get; set; }

        [Required]
        public string DurationType { get; set; }

        [Required]
        public string SeatingArea { get; set; }

        public List<Reservation> Pending { get; set; } = new();
        public List<Reservation> Confirmed { get; set; } = new();
        public List<Reservation> Completed { get; set; } = new();
        public List<Reservation> Cancelled { get; set; } = new();
    }
}
