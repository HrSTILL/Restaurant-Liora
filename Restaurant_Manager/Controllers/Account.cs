using Microsoft.AspNetCore.Mvc;

namespace Restaurant_Manager.Controllers
{
    public class Account : Controller
    {
        public IActionResult CustomerAccount()
        {
            return View();
        }
    }
}
