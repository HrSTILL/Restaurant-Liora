public class AdminReservationViewModel
{
    public int UserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public List<Reservation> Reservations { get; set; }
}
