using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant_Manager.Data;
using Restaurant_Manager.Models;
using Restaurant_Manager.Utils;
using Restaurant_Manager.ViewModels;
using System.Security.Cryptography;
using System.Text;

[Authorize(Roles = "admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult AdminDash() => View();

    public IActionResult AdminMenu()
    {
        var menuItems = _context.MenuItems.ToList();
        return View(menuItems);
    }

    [HttpGet]
    public IActionResult CreateMenuItem()
    {
        var model = PrepareMenuItemForm();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMenuItem(MenuItemFormViewModel model)
    {
        string fileName = Path.GetFileName(model.ImageFile.FileName);
        string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images_Menu_rm", fileName);

        using (var stream = new FileStream(imagePath, FileMode.Create))
        {
            await model.ImageFile.CopyToAsync(stream);
        }

        var item = new MenuItem
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            ImageUrl = "/Images_Menu_rm/" + fileName,
            Category = model.Category,
            Allergens = string.Join(",", model.Allergens ?? new List<string>()),
            Calories = model.Calories,
            IsGlutenFree = model.IsGlutenFree,
            PrepTimeMinutes = model.PrepTime,
            Tags = model.Tags,
            CreatedAt = DateTime.UtcNow
        };

        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Menu item created successfully!";
        return RedirectToAction("AdminMenu");
    }


    [HttpGet]
    public async Task<IActionResult> EditMenuItem(int id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null) return NotFound();

        var model = new MenuItemFormViewModel
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            ExistingImageUrl = item.ImageUrl,
            Category = item.Category,
            Allergens = item.Allergens?.Split(',').ToList() ?? new List<string>(),
            Calories = item.Calories,
            IsGlutenFree = item.IsGlutenFree,
            PrepTime = item.PrepTimeMinutes,
            Tags = item.Tags,
            CategoryOptions = GetCategoryOptions(),
            AllergenOptions = GetAllergenOptions()
        };

        return View("EditMenuItem", model);
    }

    [HttpPost]
    public async Task<IActionResult> EditMenuItem(MenuItemFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.CategoryOptions = GetCategoryOptions();
            model.AllergenOptions = GetAllergenOptions();
            return View(model);
        }

        var item = await _context.MenuItems.FindAsync(model.Id);
        if (item == null) return NotFound();

        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            string fileName = Path.GetFileName(model.ImageFile.FileName);
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images_Menu_rm", fileName);

            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(stream);
            }

            item.ImageUrl = "/Images_Menu_rm/" + fileName;
        }

        item.Name = model.Name;
        item.Description = model.Description;
        item.Price = model.Price;
        item.Category = model.Category;
        item.Allergens = string.Join(",", model.Allergens ?? new List<string>());
        item.Calories = model.Calories;
        item.IsGlutenFree = model.IsGlutenFree;
        item.PrepTimeMinutes = model.PrepTime;
        item.Tags = model.Tags;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Menu item updated successfully!";
        return RedirectToAction("AdminMenu");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMenuItemAjax(int id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null) return NotFound();

        _context.MenuItems.Remove(item);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetMenuItemById(int id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null) return NotFound();

        var model = new
        {
            id = item.Id,
            name = item.Name,
            description = item.Description,
            price = item.Price,
            category = item.Category,
            tags = item.Tags
        };

        return Json(model);
    }

    public IActionResult AdminSpecialOffers()
    {
        var specials = _context.MenuItems
            .Where(x => x.Category.ToLower() == "special")
            .ToList();

        return View(specials);
    }

    [HttpGet]
    public IActionResult CreateSpecialOffer()
    {
        var model = PrepareMenuItemForm();
        model.Category = "special";
        return View("CreateSpecialOffer", model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSpecialOffer(MenuItemFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.CategoryOptions = GetCategoryOptions();
            model.AllergenOptions = GetAllergenOptions();
            return View("CreateSpecialOffer", model);
        }

        string fileName = Path.GetFileName(model.ImageFile.FileName);
        string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images_Menu_rm", fileName);

        using (var stream = new FileStream(imagePath, FileMode.Create))
        {
            await model.ImageFile.CopyToAsync(stream);
        }

        var item = new MenuItem
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            ImageUrl = "/Images_Menu_rm/" + fileName,
            Category = "special",
            Allergens = string.Join(",", model.Allergens ?? new List<string>()),
            Calories = model.Calories,
            IsGlutenFree = model.IsGlutenFree,
            PrepTimeMinutes = model.PrepTime,
            Tags = model.Tags,
            CreatedAt = DateTime.UtcNow
        };

        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Special offer created successfully!";
        return RedirectToAction("AdminSpecialOffers");
    }

    [HttpGet]
    public async Task<IActionResult> EditSpecialOffer(int id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null || item.Category.ToLower() != "special") return NotFound();

        var model = new MenuItemFormViewModel
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            ExistingImageUrl = item.ImageUrl,
            Category = item.Category,
            Allergens = item.Allergens?.Split(',').ToList() ?? new List<string>(),
            Calories = item.Calories,
            IsGlutenFree = item.IsGlutenFree,
            PrepTime = item.PrepTimeMinutes,
            Tags = item.Tags,
            CategoryOptions = GetCategoryOptions(),
            AllergenOptions = GetAllergenOptions()
        };

        return View("EditSpecialOffer", model);
    }

    [HttpPost]
    public async Task<IActionResult> EditSpecialOffer(MenuItemFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.CategoryOptions = GetCategoryOptions();
            model.AllergenOptions = GetAllergenOptions();
            return View("EditSpecialOffer", model);
        }

        var item = await _context.MenuItems.FindAsync(model.Id);
        if (item == null || item.Category.ToLower() != "special") return NotFound();

        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            string fileName = Path.GetFileName(model.ImageFile.FileName);
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images_Menu_rm", fileName);

            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await model.ImageFile.CopyToAsync(stream);
            }

            item.ImageUrl = "/Images_Menu_rm/" + fileName;
        }

        item.Name = model.Name;
        item.Description = model.Description;
        item.Price = model.Price;
        item.Allergens = string.Join(",", model.Allergens ?? new List<string>());
        item.Calories = model.Calories;
        item.IsGlutenFree = model.IsGlutenFree;
        item.PrepTimeMinutes = model.PrepTime;
        item.Tags = model.Tags;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Special offer updated successfully!";
        return RedirectToAction("AdminSpecialOffers");
    }
    public async Task<IActionResult> AdminOrders(string search = "")
    {
        var ordersQuery = _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            ordersQuery = ordersQuery.Where(o =>
                o.User.FirstName.Contains(search) ||
                o.User.LastName.Contains(search) ||
                o.User.Email.Contains(search));
        }

        var orders = await ordersQuery
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> AdminReservations(string search = "")
    {
        var reservationsQuery = _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Table)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            reservationsQuery = reservationsQuery.Where(r =>
                r.User.FirstName.Contains(search) ||
                r.User.LastName.Contains(search) ||
                r.User.Email.Contains(search));
        }

        var reservations = await reservationsQuery
            .OrderByDescending(r => r.ReservationTime)
            .ToListAsync();

        return View(reservations);
    }

    public async Task<IActionResult> ManageUsers(int page = 1, string search = "")
    {
        var query = _context.Users.Where(u => u.Role == "customer");

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u =>
                u.Username.Contains(search) ||
                u.Email.Contains(search) ||
                u.FirstName.Contains(search) ||
                u.LastName.Contains(search));
        }

        var users = await PaginatedList<UserViewModel>.CreateAsync(
            query.Select(u => new UserViewModel
            {
                Id = u.Id,
                Username = u.Username,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role
            }),
            page, 10);

        ViewData["Search"] = search;
        return View(users);
    }

    public async Task<IActionResult> ManageStaff(int page = 1, string search = "")
    {
        var query = _context.Users.Where(u => u.Role == "staff");

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u =>
                u.Username.Contains(search) ||
                u.Email.Contains(search) ||
                u.FirstName.Contains(search) ||
                u.LastName.Contains(search));
        }

        var staff = await PaginatedList<UserViewModel>.CreateAsync(
            query.Select(u => new UserViewModel
            {
                Id = u.Id,
                Username = u.Username,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role
            }),
            page, 10);

        ViewData["Search"] = search;
        return View(staff);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (!ModelState.IsValid) return BadRequest("Invalid data");

        var user = new User
        {
            Username = model.Username,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Phone = model.Phone,
            PasswordHash = HashPassword(model.Password),
            Role = "customer"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> CreateStaff(CreateUserViewModel model)
    {
        if (!ModelState.IsValid) return BadRequest("Invalid data");

        var user = new User
        {
            Username = model.Username,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Phone = model.Phone,
            PasswordHash = HashPassword(model.Password),
            Role = "staff"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(EditUserViewModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _context.Users.FindAsync(model.Id);
        if (user == null) return NotFound();

        user.Username = model.Username;
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Email = model.Email;
        user.Phone = model.Phone;

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser([FromBody] int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Ok();
    }


    [HttpGet]
    public async Task<IActionResult> UserOrders(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.MenuItem)
            .Where(o => o.UserId == id)
            .ToListAsync();

        var viewModel = new CustomerOrdersViewModel
        {
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            Phone = user.Phone,
            Pending = orders.Where(o => o.Status == "pending").ToList(),
            Preparing = orders.Where(o => o.Status == "preparing").ToList(),
            Served = orders.Where(o => o.Status == "served").ToList(),
            Completed = orders.Where(o => o.Status == "completed").ToList(),
            Cancelled = orders.Where(o => o.Status == "cancelled").ToList()
        };

        return View("UserOrders", viewModel);
    }





    [HttpGet]
    public async Task<IActionResult> UserReservations(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        var reservations = await _context.Reservations
            .Include(r => r.Table)
            .Where(r => r.UserId == id)
            .ToListAsync();

        var viewModel = new CustomerReservationsViewModel
        {
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            PhoneNumber = user.Phone,
            Pending = reservations.Where(r => r.Status == "pending").ToList(),
            Confirmed = reservations.Where(r => r.Status == "confirmed").ToList(),
            Completed = reservations.Where(r => r.Status == "completed").ToList(),
            Cancelled = reservations.Where(r => r.Status == "cancelled").ToList()
        };

        return View("UserReservations", viewModel);
    }





    [HttpGet]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone
        };

        return Json(model);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private MenuItemFormViewModel PrepareMenuItemForm()
    {
        return new MenuItemFormViewModel
        {
            CategoryOptions = GetCategoryOptions(),
            AllergenOptions = GetAllergenOptions()
        };
    }

    private List<SelectListItem> GetCategoryOptions() => new()
    {
        new SelectListItem { Value = "salad", Text = "Salads and Appetizers" },
        new SelectListItem { Value = "main", Text = "Main Courses" },
        new SelectListItem { Value = "dessert", Text = "Desserts" },
        new SelectListItem { Value = "drink", Text = "Drinks" }
    };

    private List<SelectListItem> GetAllergenOptions() => new()
    {
        new SelectListItem { Value = "Gluten", Text = "Gluten" },
        new SelectListItem { Value = "Nuts", Text = "Nuts" },
        new SelectListItem { Value = "Dairy", Text = "Dairy" },
        new SelectListItem { Value = "Egg", Text = "Egg" },
        new SelectListItem { Value = "Soy", Text = "Soy" },
        new SelectListItem { Value = "Fish", Text = "Fish" },
        new SelectListItem { Value = "Peanuts", Text = "Peanuts" },
        new SelectListItem { Value = "Sesame", Text = "Sesame" }
    };

    [HttpGet]
    public JsonResult GetDashboardSummary()
    {
        var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        var totalRevenue = _context.Orders
            .Where(o => o.Status == "completed" && o.CreatedAt >= startOfMonth)
            .Sum(o => (decimal?)o.TotalPrice) ?? 0;

        var totalOrders = _context.Orders
            .Count(o => o.Status == "completed" && o.CreatedAt >= startOfMonth);

        var totalStaff = _context.Users.Count(u => u.Role == "staff");

        var totalSpecials = _context.MenuItems
            .Count(mi => mi.Category.ToLower() == "special");

        var totalUsers = _context.Users.Count(u => u.Role == "customer");

        var totalReservations = _context.Reservations
            .Count(r => r.Status != "cancelled" && r.ReservationTime >= startOfMonth);

        return Json(new
        {
            totalRevenue,
            totalOrders,
            totalStaff,
            totalSpecials,
            totalUsers,
            totalReservations
        });
    }

    [HttpGet]
    public JsonResult GetDashboardRevenueChart()
    {
        var last7Days = DateTime.Now.AddDays(-6);

        var revenueData = _context.Orders
            .Where(o => o.Status == "completed" && o.CreatedAt.Date >= last7Days)
            .AsEnumerable()
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                TotalRevenue = g.Sum(x => x.TotalPrice)
            })
            .OrderBy(x => x.Date)
            .ToList();

        return Json(revenueData);
    }

    [HttpGet]
    public JsonResult GetDashboardReservationChart()
    {
        var last7Days = DateTime.Now.AddDays(-6);

        var reservationData = _context.Reservations
            .Where(r => r.Status != "cancelled" && r.ReservationTime.Date >= last7Days)
            .AsEnumerable()
            .GroupBy(r => r.ReservationTime.Date)
            .Select(g => new
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Count = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToList();

        return Json(reservationData);
    }

    [HttpGet]
    public JsonResult GetTopMenuItems()
    {
        var topItems = _context.OrderItems
            .GroupBy(oi => oi.MenuItem.Name)
            .Select(g => new
            {
                ItemName = g.Key,
                Quantity = g.Sum(oi => oi.Quantity)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(5)
            .ToList();

        return Json(topItems);
    }

    [HttpGet]
    public JsonResult GetRecentOrders()
    {
        var recentOrders = _context.Orders
            .Where(o => o.Status == "completed")
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Select(o => new
            {
                CustomerName = o.User.FirstName + " " + o.User.LastName,
                Total = o.TotalPrice,
                Date = o.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            })
            .ToList();

        return Json(recentOrders);
    }

    [HttpGet]
    public JsonResult GetUpcomingReservations()
    {
        var upcomingReservations = _context.Reservations
            .Where(r => r.Status != "cancelled" && r.ReservationTime >= DateTime.Now)
            .OrderBy(r => r.ReservationTime)
            .Take(5)
            .Select(r => new
            {
                CustomerName = r.Name,
                People = r.NumberOfPeople,
                Time = r.ReservationTime.ToString("yyyy-MM-dd HH:mm")
            })
            .ToList();

        return Json(upcomingReservations);
    }

    [HttpGet]
    public JsonResult GetRecentActivities()
    {
        var activities = new List<string>();

        var recentOrders = _context.Orders
            .Where(o => o.Status == "completed")
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .Take(3)
            .Select(o => $"🛒 Order completed - {o.User.FirstName} {o.User.LastName} - {o.TotalPrice} лв")
            .ToList();

        var recentReservations = _context.Reservations
            .Where(r => r.Status != "cancelled")
            .OrderByDescending(r => r.ReservationTime)
            .Take(2)
            .Select(r => $"📅 Reservation - {r.Name} for {r.NumberOfPeople} people")
            .ToList();

        activities.AddRange(recentOrders);
        activities.AddRange(recentReservations);

        return Json(activities);
    }



}
