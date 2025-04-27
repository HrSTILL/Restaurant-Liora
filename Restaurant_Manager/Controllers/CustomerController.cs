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
        // (EN) Loads the main customer page | (BG) Зарежда главната страница на клиента
        public IActionResult Index()
        {
            return View();
        }
        // (EN) Loads the menu page | (BG) Зарежда страницата с менюто
        public IActionResult Menu()
        {
            var menuItems = _context.MenuItems.ToList();

            var grouped = menuItems
                .GroupBy(item => item.Category)
                .ToDictionary(g => g.Key, g => g.ToList());

            return View(grouped);
        }
        // (EN) Loads the special offers page | (BG) Зарежда страницата с промоции
        public IActionResult SpecialOffers()
        {
            var specials = _context.MenuItems
          .Where(x => x.Category.ToLower() == "special")
          .ToList();

            return View(specials);
        }
    }
}
