using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "staff")]
public class StaffController : Controller
{
    public IActionResult StaffDash()
    {
        return View();
    }
}
