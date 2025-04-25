namespace Restaurant_Manager.ViewModels
{
    public class StaffReservationUserViewModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string TableNumbers { get; set; } = string.Empty;
    }
}
