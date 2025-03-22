using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "staff")]
public class StaffController : Controller
{
    public IActionResult StaffDash()
    {
        return View();
    }

    public IActionResult StaffOrders()
    {
        return View();
    }

    public IActionResult StaffReservations()
    {
        return View();
    }
}
