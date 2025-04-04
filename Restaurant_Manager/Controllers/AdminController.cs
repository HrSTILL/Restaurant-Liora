using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "admin")]
public class AdminController : Controller
{
    public IActionResult AdminDash()
    {
        return View();
    }

    public IActionResult AdminMenu()
    {
        return View();
    }

    public IActionResult AdminOrders()
    {
        return View();
    }

    public IActionResult AdminReports()
    {
        return View();
    }

    public IActionResult AdminReservations()
    {
        return View();
    }

    public IActionResult AdminSpecialOffers()
    {
        return View();
    }

    public IActionResult AdminTables()
    {
        return View();
    }

    public IActionResult ManageStaff()
    {
        return View();
    }

    public IActionResult ManageUsers()
    {
        return View();
    }
}
