using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Restaurant_Manager.Data;

public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public ActionResult AdminReports()
    {
        return View();
    }

    [HttpGet]
    public JsonResult GetRevenueData(string mode = "last7")
    {
        var query = _context.Orders
            .Where(o => o.Status == "completed");

        IEnumerable<object> result;

        switch (mode)
        {
            case "last7":
                var last7 = DateTime.Now.Date.AddDays(-6);
                result = query
                    .Where(o => o.CreatedAt.Date >= last7 && o.CreatedAt.Date <= DateTime.Today)
                    .GroupBy(o => o.CreatedAt.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalRevenue = g.Sum(o => o.TotalPrice)
                    })
                    .OrderBy(x => x.Date)
                    .AsEnumerable()
                    .Select(x => new {
                        Date = x.Date.ToString("yyyy-MM-dd"),
                        TotalRevenue = x.TotalRevenue
                    });
                break;

            case "top7":
                var currentMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var currentMonthEnd = currentMonthStart.AddMonths(1);

                result = query
                    .Where(o => o.CreatedAt >= currentMonthStart && o.CreatedAt < currentMonthEnd)
                    .GroupBy(o => o.CreatedAt.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalRevenue = g.Sum(o => o.TotalPrice)
                    })
                    .AsEnumerable()
                    .OrderByDescending(x => x.TotalRevenue)
                    .Take(7)
                    .OrderBy(x => x.Date)
                    .Select(x => new {
                        Date = x.Date.ToString("yyyy-MM-dd"),
                        TotalRevenue = x.TotalRevenue
                    });
                break;

            case "topDayEachMonth":
                var last6Months = Enumerable.Range(0, 6)
                    .Select(i => DateTime.Now.AddMonths(-i))
                    .ToList();

                result = last6Months
                    .Select(month =>
                        query.Where(o => o.CreatedAt.Month == month.Month && o.CreatedAt.Year == month.Year)
                            .GroupBy(o => o.CreatedAt.Date)
                            .Select(g => new {
                                Date = g.Key,
                                TotalRevenue = g.Sum(o => o.TotalPrice)
                            })
                            .AsEnumerable()
                            .OrderByDescending(x => x.TotalRevenue)
                            .FirstOrDefault()
                    )
                    .Where(x => x != null)
                    .OrderBy(x => x.Date)
                    .Select(x => new {
                        Date = x.Date.ToString("yyyy-MM-dd"),
                        TotalRevenue = x.TotalRevenue
                    });
                break;

            case "thisMonth":
                var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var today = DateTime.Now.Date;
                result = query
                    .Where(o => o.CreatedAt.Date >= startOfMonth && o.CreatedAt.Date <= today)
                    .GroupBy(o => o.CreatedAt.Date)
                    .Select(g => new {
                        Date = g.Key,
                        TotalRevenue = g.Sum(o => o.TotalPrice)
                    })
                    .OrderBy(x => x.Date)
                    .AsEnumerable()
                    .Select(x => new {
                        Date = x.Date.ToString("yyyy-MM-dd"),
                        TotalRevenue = x.TotalRevenue
                    });
                break;

            default:
                result = Enumerable.Empty<object>();
                break;
        }

        return Json(result);
    }

    [HttpGet]
    public JsonResult GetReservationData(string mode = "last7")
    {
        var query = _context.Reservations
            .Where(r => r.Status != "cancelled");

        IEnumerable<object> result;

        switch (mode)
        {
            case "last7":
                var last7 = DateTime.Now.Date.AddDays(-6);
                result = query
                    .Where(r => r.ReservationTime.Date >= last7)
                    .GroupBy(r => r.ReservationTime.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .AsEnumerable()
                    .Select(x => new
                    {
                        Date = x.Date.ToString("yyyy-MM-dd"),
                        Count = x.Count
                    });
                break;

            case "thisMonth":
                var start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                result = query
                    .Where(r => r.ReservationTime >= start)
                    .GroupBy(r => r.ReservationTime.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .AsEnumerable()
                    .Select(x => new
                    {
                        Date = x.Date.ToString("yyyy-MM-dd"),
                        Count = x.Count
                    });
                break;

            case "top7":
                result = query
                    .Where(r => r.ReservationTime.Month == DateTime.Now.Month &&
                                r.ReservationTime.Year == DateTime.Now.Year)
                    .GroupBy(r => r.ReservationTime.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .AsEnumerable()
                    .OrderByDescending(x => x.Count)
                    .Take(7)
                    .OrderBy(x => x.Date)
                    .Select(x => new
                    {
                        Date = x.Date.ToString("yyyy-MM-dd"),
                        Count = x.Count
                    });
                break;

            case "topDayEachMonth":
                var last6Months = Enumerable.Range(0, 6)
                    .Select(i => DateTime.Now.AddMonths(-i))
                    .ToList();

                result = last6Months
                    .Select(month =>
                        query.Where(r => r.ReservationTime.Month == month.Month &&
                                         r.ReservationTime.Year == month.Year)
                             .GroupBy(r => r.ReservationTime.Date)
                             .Select(g => new
                             {
                                 Date = g.Key,
                                 Count = g.Count()
                             })
                             .AsEnumerable()
                             .OrderByDescending(x => x.Count)
                             .FirstOrDefault()
                    )
                    .Where(x => x != null)
                    .OrderBy(x => x.Date)
                    .Select(x => new
                    {
                        Date = x.Date.ToString("yyyy-MM-dd"),
                        Count = x.Count
                    });
                break;

            default:
                result = Enumerable.Empty<object>();
                break;
        }

        return Json(result);
    }

    [HttpGet]
    public JsonResult GetPopularItemsData()
    {
        var popularItems = _context.OrderItems
            .GroupBy(oi => oi.MenuItem.Name)
            .Select(g => new
            {
                ItemName = g.Key,
                Quantity = g.Sum(oi => oi.Quantity)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(7) 
            .ToList();

        return Json(popularItems);
    }

    [HttpGet]
    public JsonResult GetSummaryCardsData()
    {
        var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var today = DateTime.Today;

        var completedOrdersThisMonth = _context.Orders
            .Where(o => o.Status == "completed" && o.CreatedAt.Date >= startOfMonth && o.CreatedAt.Date <= today)
            .ToList(); 

        var totalRevenue = completedOrdersThisMonth.Sum(o => o.TotalPrice);
        var totalOrders = completedOrdersThisMonth.Count;

        var totalReservations = _context.Reservations
            .Where(r => r.Status != "cancelled" && r.ReservationTime.Date >= startOfMonth && r.ReservationTime.Date <= today)
            .Count();

        var topDay = completedOrdersThisMonth
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Revenue = g.Sum(o => o.TotalPrice)
            })
            .OrderByDescending(x => x.Revenue)
            .FirstOrDefault()?.Date.ToString("yyyy-MM-dd") ?? "--";

        return Json(new
        {
            totalRevenue = Math.Round(totalRevenue, 2),
            totalOrders,
            totalReservations,
            topDay
        });
    }

}
