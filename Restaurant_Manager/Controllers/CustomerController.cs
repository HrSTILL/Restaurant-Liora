using Microsoft.AspNetCore.Mvc;

namespace Restaurant_Manager.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Menu()
        {
            return View();
        }
        public IActionResult SpecialOffers()
        {
            return View();
        }
        public IActionResult CustomerCart()
        {
            return View();
        }
    }
}
