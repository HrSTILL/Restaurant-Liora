using Restaurant_Manager.Models;

public class AdminOrderViewModel
{
    public int UserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public List<Order> Orders { get; set; }
}
