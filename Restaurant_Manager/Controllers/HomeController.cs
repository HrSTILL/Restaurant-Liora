using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Restaurant_Manager.Models;
using Restaurant_Manager.Data;
using System.Linq;

namespace Restaurant_Manager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}
