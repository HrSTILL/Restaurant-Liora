using Microsoft.AspNetCore.Mvc;

namespace Restaurant_Manager.Controllers
{
    public class Reservations : Controller
    {
        public IActionResult CustomerReservations()
        {
            return View();
        }
    }
}
