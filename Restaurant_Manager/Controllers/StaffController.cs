using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Manager.Data;
using Restaurant_Manager.ViewModels;

[Authorize(Roles = "staff")]
public class StaffController : Controller
{
    private readonly ApplicationDbContext _context;

    public StaffController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult StaffDash()
    {
        return View();
    }

    public IActionResult StaffReports()
    {
        return View();
    }

    [HttpGet]
    public JsonResult GetStaffSummary()
    {
        var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var today = DateTime.Today;

        var totalRevenue = _context.Orders
            .Where(o => o.Status == "completed" && o.CreatedAt.Date >= startOfMonth && o.CreatedAt.Date <= today)
            .Sum(o => (decimal?)o.TotalPrice) ?? 0;

        var totalOrders = _context.Orders
            .Count(o => o.Status == "completed" && o.CreatedAt.Date >= startOfMonth && o.CreatedAt.Date <= today);

        var totalReservations = _context.Reservations
            .Count(r => r.Status != "cancelled" && r.ReservationTime.Date >= startOfMonth && r.ReservationTime.Date <= today);

        return Json(new
        {
            totalRevenue,
            totalOrders,
            totalReservations
        });
    }


    [HttpGet]
    public JsonResult GetStaffRevenueChart()
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
    public JsonResult GetStaffReservationChart()
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
    public async Task<IActionResult> StaffOrders()
    {
        var usersWithOrders = await _context.Orders
            .Where(o => o.Status == "pending" || o.Status == "preparing" || o.Status == "served")
            .Include(o => o.User)
            .GroupBy(o => o.UserId)
            .Select(g => new StaffOrderUserViewModel
            {
                UserId = g.Key,
                FirstName = g.First().User.FirstName,
                LastName = g.First().User.LastName,
                Email = g.First().User.Email,
                Phone = g.First().User.Phone
            })
            .ToListAsync();

        return View(usersWithOrders);
    }

    [HttpGet]
    public async Task<IActionResult> StaffReservations()
    {
        var reservations = await _context.Reservations
            .Where(r => r.Status == "pending" || r.Status == "confirmed")
            .Include(r => r.User)
            .Include(r => r.Table)
            .ToListAsync(); 

        var usersWithReservations = reservations
            .GroupBy(r => r.UserId)
            .Select(g => new StaffReservationUserViewModel
            {
                UserId = g.Key,
                FirstName = g.First().User.FirstName,
                LastName = g.First().User.LastName,
                Email = g.First().User.Email,
                Phone = g.First().User.Phone,
                TableNumbers = string.Join(", ", g.Select(x => x.Table.TableNumber.ToString()).Distinct())
            })
            .ToList();

        return View(usersWithReservations);
    }


    [HttpGet]
    public async Task<IActionResult> StaffUserOrders(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
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

        return View("StaffUserOrders", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> StaffUserReservations(int id)
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

        return View("StaffUserReservations", viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateOrderStatus(int id, string newStatus)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();

        order.Status = newStatus;
        await _context.SaveChangesAsync();

        TempData["ToastSuccess"] = "Status updated Successfully.";
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateReservationStatus(int id, string newStatus)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null) return NotFound();

        reservation.Status = newStatus;
        await _context.SaveChangesAsync();

        TempData["ToastSuccess"] = "Status updated Successfully.";
        return Ok();
    }
}
