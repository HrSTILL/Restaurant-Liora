using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Manager.Data;

namespace Restaurant_Manager.Controllers
{

    public class CustomerController : Controller
    {

        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Menu()
        {
            var menuItems = _context.MenuItems.ToList();

            var grouped = menuItems
                .GroupBy(item => item.Category)
                .ToDictionary(g => g.Key, g => g.ToList());

            return View(grouped);
        }
        public IActionResult SpecialOffers()
        {
            var specials = _context.MenuItems
          .Where(x => x.Category.ToLower() == "special")
          .ToList();

            return View(specials);
        }
        public IActionResult CustomerCart()
        {
            return View();
        }
    }
}
