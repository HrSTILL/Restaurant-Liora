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
using Restaurant_Manager.Models.ViewModels;
using System.Globalization;

[Authorize(Roles = "admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }
    // (EN) Loads the admin dashboard page | (BG) Запълва страницата на админ таблото
    public IActionResult AdminDash() => View();

    // (EN) Loads the admin menu page | (BG) Запълва страницата на админ менюто
    public IActionResult AdminMenu()
    {
        var menuItems = _context.MenuItems.ToList();
        return View(menuItems);
    }

    // (EN) Loads the create menu item form | (BG) Запълва формата за създаване на меню
    [HttpGet]
    public IActionResult CreateMenuItem()
    {
        var model = PrepareMenuItemForm();
        return View(model);
    }

    // (EN) Handles the creation of a new menu item | (BG) Обработва създаването на ново меню
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

        TempData["ToastSuccess"] = "Menu item created successfully!";
        return RedirectToAction("AdminMenu");
    }

    // (EN) Loads the edit menu item form | (BG) Запълва формата за редактиране на меню
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

    // (EN) Handles the editing of an existing menu item | (BG) Обработва редактирането на съществуващо меню
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

        TempData["ToastSuccess"] = "Menu item updated successfully!";
        return RedirectToAction("AdminMenu");
    }

    // (EN) Handles the deletion of a menu item | (BG) Обработва изтриването на меню
    [HttpPost]
    public async Task<IActionResult> DeleteMenuItemAjax(int id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null) return NotFound();

        _context.MenuItems.Remove(item);
        await _context.SaveChangesAsync();
        TempData["ToastSuccess"] = "Menu item deleted successfully!";
        return Ok();
    }

    // (EN) Loads the details of a menu item | (BG) Запълва детайлите на меню
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

    // (EN) Loads the admin special offers page | (BG) Запълва страницата на админ специални оферти
    public IActionResult AdminSpecialOffers()
    {
        var specials = _context.MenuItems
            .Where(x => x.Category.ToLower() == "special")
            .ToList();

        return View(specials);
    }

    // (EN) Loads the create special offer form | (BG) Запълва формата за създаване на специална оферта
    [HttpGet]
    public IActionResult CreateSpecialOffer()
    {
        var model = PrepareMenuItemForm();
        model.Category = "special";
        return View("CreateSpecialOffer", model);
    }

    // (EN) Handles the creation of a new special offer | (BG) Обработва създаването на нова специална оферта
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

        TempData["ToastSuccess"] = "Special offer created successfully!";
        return RedirectToAction("AdminSpecialOffers");
    }

    // (EN) Loads the edit special offer form | (BG) Запълва формата за редактиране на специална оферта
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

    // (EN) Handles the editing of an existing special offer | (BG) Обработва редактирането на съществуваща специална оферта
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

        TempData["ToastSuccess"] = "Special offer updated successfully!";
        return RedirectToAction("AdminSpecialOffers");
    }
    // (EN) Handles the deletion of a special offer | (BG) Обработва изтриването на специална оферта
    [HttpGet]
    public async Task<IActionResult> AdminOrders(string search = "", int page = 1)
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

        var orders = await PaginatedList<Order>.CreateAsync(
            ordersQuery.OrderByDescending(o => o.CreatedAt),
            page, 25);

        return View(orders);
    }

    // (EN) Loads the admin reservations page | (BG) Запълва страницата на админ резервации
    [HttpGet]
    public async Task<IActionResult> AdminReservations(string search = "", int page = 1)
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

        var reservations = await PaginatedList<Reservation>.CreateAsync(
            reservationsQuery.OrderByDescending(r => r.ReservationTime),
            page, 25);

        return View(reservations);
    }
    // (EN) Updates the status of an order | (BG) Обновява статуса на поръчка
    [HttpPost]
    public async Task<IActionResult> UpdateOrderStatus(int id, string newStatus)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();

        order.Status = newStatus;
        await _context.SaveChangesAsync();

        TempData["ToastSuccess"] = "Status updated successfully!";
        return Ok();
    }
    // (EN) Updates the status of a reservation | (BG) Обновява статуса на резервация
    [HttpPost]
    public async Task<IActionResult> UpdateReservationStatus(int id, string newStatus)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null) return NotFound();

        reservation.Status = newStatus;
        await _context.SaveChangesAsync();

        TempData["ToastSuccess"] = "Status updated successfully!";
        return Ok();
    }
    // (EN) Loads the admin income page | (BG) Запълва страницата на админ приходи
    public IActionResult AdminIncome(int page = 1, int? year = null)
    {
        var completedOrders = _context.Orders
            .Where(o => o.Status.ToLower() == "completed")
            .ToList();

        var minDate = completedOrders.Any() ? completedOrders.Min(o => o.CreatedAt.Date) : DateTime.Today;
        var maxDate = DateTime.Today;

        var groupedIncome = completedOrders
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new DailyIncome
            {
                Date = g.Key,
                TotalIncome = g.Sum(x => x.TotalPrice)
            })
            .ToList();

        var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var today = DateTime.Now.Date;

        var viewModel = new AdminIncomeViewModel
        {
            TotalIncome = completedOrders.Sum(x => x.TotalPrice),
            IncomeToday = completedOrders
                .Where(x => x.CreatedAt.Date == DateTime.Today)
                .Sum(x => x.TotalPrice),
            IncomeThisMonth = completedOrders
                .Where(x => x.CreatedAt.Date >= startOfMonth && x.CreatedAt.Date <= today)
                .Sum(x => x.TotalPrice), 
            HighestIncomeDay = groupedIncome.OrderByDescending(x => x.TotalIncome).FirstOrDefault(),
            DailyIncomes = PaginatedList<DailyIncome>.Create(groupedIncome.OrderByDescending(x => x.Date).ToList(), page, 10),
            Last10DaysIncome = groupedIncome.OrderBy(x => x.Date).TakeLast(10).ToList(),
            AvailableYears = completedOrders
                .Select(o => o.CreatedAt.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList(),
            SelectedYear = year ?? DateTime.Today.Year,
            MonthlyIncomes = completedOrders
                .Where(o => o.CreatedAt.Year == (year ?? DateTime.Today.Year))
                .GroupBy(o => o.CreatedAt.Month)
                .Select(g => new MonthlyIncome
                {
                    Month = g.Key,
                    TotalIncome = g.Sum(x => x.TotalPrice)
                })
                .OrderBy(m => m.Month)
                .ToList()
        };

        return View(viewModel);
    }

    // (EN) Loads the admin users page | (BG) Запълва страницата на админ потребители
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
    // (EN) Loads the admin staff page | (BG) Запълва страницата на админ служители
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
    // (EN) Loads the create user form | (BG) Запълва формата за създаване на потребител
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
        TempData["ToastSuccess"] = "User created successfully!";
        return Ok();
    }
    // (EN) Loads the create staff form | (BG) Запълва формата за създаване на служител
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
        TempData["ToastSuccess"] = "Staff created successfully!";
        return Ok();
    }
    // (EN) Loads the edit user/staff form | (BG) Запълва формата за редактиране на потребител/служител
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
        if(user.Role == "customer")
        TempData["ToastSuccess"] = "User updated successfully.";
        if(user.Role == "staff")
        TempData["ToastSuccess"] = "Staff updated successfully.";
        return Ok();
    }
    // (EN) Handles the deletion of a user/staff | (BG) Обработва изтриването на потребител/служител
    [HttpPost]
    public async Task<IActionResult> DeleteUser([FromBody] int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        if (user.Role == "customer")
            TempData["ToastSuccess"] = "User deleted successfully.";
        if (user.Role == "staff")
            TempData["ToastSuccess"] = "Staff deleted successfully.";
        return Ok();
    }

    // (EN) Loads the user orders page | (BG) Запълва страницата на поръчките на потребителя
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
    // (EN) Loads the user reservations page | (BG) Запълва страницата на резервациите на потребителя
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
    // (EN) Gets users by ID | (BG) Взима потребители по ID
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
    // (EN) Hashes the password | (BG) Хешира паролата
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
    // (EN) Prepares the menu item form | (BG) Подготвя формата за меню
    private MenuItemFormViewModel PrepareMenuItemForm()
    {
        return new MenuItemFormViewModel
        {
            CategoryOptions = GetCategoryOptions(),
            AllergenOptions = GetAllergenOptions()
        };
    }
    //(EN) Gets the category options | (BG) Взима категориите
    private List<SelectListItem> GetCategoryOptions() => new()
    {
        new SelectListItem { Value = "salad", Text = "Salads and Appetizers" },
        new SelectListItem { Value = "main", Text = "Main Courses" },
        new SelectListItem { Value = "dessert", Text = "Desserts" },
        new SelectListItem { Value = "drink", Text = "Drinks" }
    };
    // (EN) Gets the allergen options | (BG) Взима алергените
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

    // (EN) Gets the dashboard summary | (BG) Взима резюмето на таблото
    [HttpGet]
    public JsonResult GetDashboardSummary()
    {
        var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var today = DateTime.Now.Date;

        var totalRevenue = _context.Orders
            .Where(o => o.Status.ToLower() == "completed" && o.CreatedAt.Date >= startOfMonth && o.CreatedAt.Date <= today)
            .Sum(o => (decimal?)o.TotalPrice) ?? 0;

        var totalOrders = _context.Orders
            .Count(o => o.Status.ToLower() == "completed" && o.CreatedAt.Date >= startOfMonth && o.CreatedAt.Date <= today);

        var totalStaff = _context.Users.Count(u => u.Role == "staff");
        var totalSpecials = _context.MenuItems.Count(mi => mi.Category.ToLower() == "special");
        var totalUsers = _context.Users.Count(u => u.Role == "customer");
        var totalReservations = _context.Reservations
            .Count(r => r.Status.ToLower() != "cancelled" && r.ReservationTime.Date >= startOfMonth && r.ReservationTime.Date <= today);

        return Json(new
        {
            totalRevenue = Math.Round(totalRevenue, 2),
            totalOrders,
            totalStaff,
            totalSpecials,
            totalUsers,
            totalReservations
        });
    }

    // (EN) Gets the dashboard revenue chart | (BG) Взима графиката на приходите
    [HttpGet]
    public JsonResult GetDashboardRevenueChart()
    {
        var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);

        var revenueData = _context.Orders
            .Where(o => o.Status == "completed" && o.CreatedAt >= startOfMonth && o.CreatedAt < endOfMonth)
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
    // (EN) Gets the dashboard reservation chart | (BG) Взима графиката на резервациите
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
    // (EN) Gets the top menu items | (BG) Взима топ менюта
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
    // (EN) Gets the recent orders | (BG) Взима последните поръчки
    [HttpGet]
    public JsonResult GetRecentOrders()
    {
        var recentOrders = _context.Orders
            .Where(o => o.Status == "completed")
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .AsEnumerable()
            .Select(o => new
            {
                CustomerName = o.User != null ? (o.User.FirstName + " " + o.User.LastName) : "Unknown",
                Total = o.TotalPrice
            })
            .ToList();

        return Json(recentOrders);
    }
    // (EN) Gets the upcoming reservations | (BG) Взима предстоящите резервации
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
    // (EN) Gets the recent activities | (BG) Взима последните активности
    public JsonResult GetRecentActivities()
    {
        var activities = new List<string>();

        var recentOrders = _context.Orders
            .Where(o => o.Status == "completed")
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .Take(3)
            .AsEnumerable()
            .Select(o => $"🛒 Order completed - {o.User.FirstName} {o.User.LastName} - {o.TotalPrice.ToString("C2")}")
            .ToList();

        var recentReservations = _context.Reservations
            .Where(r => r.Status != "cancelled")
            .OrderByDescending(r => r.ReservationTime)
            .Take(2)
            .AsEnumerable() 
            .Select(r => $"📅 Reservation - {r.Name} for {r.NumberOfPeople} people")
            .ToList();

        activities.AddRange(recentOrders);
        activities.AddRange(recentReservations);

        return Json(activities);
    }
    // (EN) Gets the daily income | (BG) Взима дневния приход
    [HttpGet]
    public JsonResult SearchDailyIncome(string query)
    {
        var completedOrders = _context.Orders
            .Where(o => o.Status.ToLower() == "completed")
            .ToList();

        var groupedIncome = completedOrders
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                TotalIncome = g.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.Date)
            .ToList();

        if (!string.IsNullOrEmpty(query))
        {
            query = query.ToLower();
            groupedIncome = groupedIncome
                .Where(x => x.Date.ToString("MMM dd, yyyy").ToLower().Contains(query))
                .ToList();
        }

        var result = groupedIncome.Select(x => new
        {
            dateFormatted = x.Date.ToString("MMM dd, yyyy"),
            totalIncomeFormatted = x.TotalIncome.ToString("C")
        });

        return Json(result);
    }

    // (EN) Gets the orders on a specific day | (BG) Взима поръчките на определен ден
    [HttpGet]
    public async Task<IActionResult> OrdersOnDay(DateTime date)
    {
        var orders = await _context.Orders
            .Where(o => o.CreatedAt.Date == date.Date && o.Status.ToLower() == "completed")
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .ToListAsync();

        var viewModel = orders.Select(o => new OrdersOnDayViewModel
        {
            OrderId = o.Id,
            FullName = $"{o.User.FirstName} {o.User.LastName}",
            Items = o.OrderItems.Select(i => new OrderItemViewModel
            {
                Name = i.MenuItem.Name,
                Price = i.MenuItem.Price,
                Quantity = i.Quantity,
            }).ToList()

        }).ToList();

        ViewData["SelectedDate"] = date.ToString("MMMM dd, yyyy");
        return View(viewModel);
    }

}
