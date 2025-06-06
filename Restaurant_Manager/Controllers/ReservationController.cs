﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Manager.Controllers;
using Restaurant_Manager.Data;
using Restaurant_Manager.Models;
using Restaurant_Manager.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

public class ReservationController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReservationController(ApplicationDbContext context)
    {
        _context = context;
    }
    // (EN) Loads the page with customers personal reservations  | (BG) Зарежда страницата с личните резервации на клиента
    public async Task<IActionResult> MyReservations()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            return RedirectToAction("Login", "Auth");

        var reservations = await _context.Reservations
            .Include(r => r.Table)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.ReservationTime)
            .ToListAsync();

        return View(new CustomerReservationsViewModel
        {
            Pending = reservations.Where(r => r.Status == "pending").ToList(),
            Confirmed = reservations.Where(r => r.Status == "confirmed").ToList(),
            Completed = reservations.Where(r => r.Status == "completed").ToList(),
            Cancelled = reservations.Where(r => r.Status == "cancelled").ToList(),
        });
    }

    // (EN) Loads the page for successful reservation  | (BG) Зарежда страницата за успешна резервация
    public IActionResult ReservationSuccess()
    {
        if (TempData["SuccessMessage"] == null)
            return RedirectToAction("CustomerReservations");

        return View();
    }

    // (EN) Loads the page for creating a reservation  | (BG) Зарежда страницата за създаване на резервация
    [HttpGet]
    public async Task<IActionResult> CustomerReservations()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            return RedirectToAction("Login", "Auth");

        var user = await _context.Users.FindAsync(userId);

        return View(new CustomerReservationsViewModel
        {
            Name = $"{user.FirstName} {user.LastName}".Trim(),
            Email = user.Email,
            PhoneNumber = user.Phone,
            SeatingArea = "Indoor"
        });
    }
    // (EN) Creates a reservation  | (BG) Създава резервация
    [HttpPost]
    public async Task<IActionResult> Create(CustomerReservationsViewModel model)
    {
        if (!ModelState.IsValid)
            return View("CustomerReservations", model);

        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            return RedirectToAction("Login", "Auth");

        var reservationTime = model.ReservationDate.Date + model.ReservationHour;

        if (reservationTime <= DateTime.Now)
        {
            ModelState.AddModelError("ReservationDate", "Reservation time cannot be in the past.");
            return View("CustomerReservations", model);
        }

        if (model.DurationType == "Extended" && model.ReservationHour >= new TimeSpan(23, 30, 0))
        {
            ModelState.AddModelError("ReservationHour", "Extended reservations must start before 11:30 PM.");
            return View("CustomerReservations", model);
        }

        if (model.DurationType == "ExtendedPlus" && model.ReservationHour >= new TimeSpan(20, 0, 0))
        {
            ModelState.AddModelError("ReservationHour", "Extended Plus reservations must start before 8:00 PM.");
            return View("CustomerReservations", model);
        }

        model.SeatingArea ??= "Indoor";

        int maxSeats = model.NumberOfPeople switch
        {
            <= 3 => 4,
            <= 5 => 6,
            <= 7 => 8,
            <= 9 => 10,
            <= 12 => 12,
            _ => int.MaxValue
        };

        TimeSpan reservationDuration = GetDuration(model.DurationType);
        var startTime = reservationTime;
        var endTime = reservationTime.Add(reservationDuration);

        var overlappingUserReservations = await _context.Reservations
            .Where(r => r.UserId == userId && r.Status != "cancelled")
            .ToListAsync();

        bool hasOverlap = overlappingUserReservations.Any(r =>
        {
            var rStart = r.ReservationTime;
            var rEnd = rStart + GetDuration(r.DurationType);
            return rStart < endTime && rEnd > startTime;
        });

        if (hasOverlap)
        {
            TempData["OverlapError"] = "❌ You already have a reservation during this time.";
            return RedirectToAction("CustomerReservations");
        }

        var availableTables = await _context.RestaurantTables
            .Where(t => t.Seats >= model.NumberOfPeople && t.Seats <= maxSeats && t.Area == model.SeatingArea)
            .OrderBy(t => t.Seats)
            .ToListAsync();

        var allReservations = await _context.Reservations
            .Where(r => r.Status != "cancelled")
            .ToListAsync();

        var reservedTableIds = allReservations
            .Where(r =>
            {
                var rStart = r.ReservationTime;
                var rEnd = rStart + GetDuration(r.DurationType);
                return rStart < endTime && rEnd > startTime;
            })
            .Select(r => r.TableId)
            .ToList();

        var freeTables = availableTables.Where(t => !reservedTableIds.Contains(t.Id)).ToList();

        if (!freeTables.Any())
        {
            string fallback = model.SeatingArea == "Indoor" ? "Outdoor" : "Indoor";

            var fallbackTables = await _context.RestaurantTables
                .Where(t => t.Seats >= model.NumberOfPeople && t.Seats <= maxSeats && t.Area == fallback)
                .OrderBy(t => t.Seats)
                .ToListAsync();

            var fallbackFreeTables = fallbackTables
                .Where(t => !reservedTableIds.Contains(t.Id))
                .ToList();

            ModelState.AddModelError("SeatingArea", fallbackFreeTables.Any()
                ? $"No {model.SeatingArea.ToLower()} tables available. Consider switching to {fallback.ToLower()}."
                : "No suitable table is available for the selected preferences.");

            return View("CustomerReservations", model);
        }

        var chosenTable = freeTables.First();

        var reservation = new Reservation
        {
            UserId = userId,
            TableId = chosenTable.Id,
            ReservationTime = reservationTime,
            NumberOfPeople = model.NumberOfPeople,
            Name = model.Name,
            PhoneNumber = model.PhoneNumber,
            Email = model.Email,
            DurationType = model.DurationType,
            SeatingArea = model.SeatingArea,
            Status = "pending"
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Reservation created successfully!";
        TempData["ToastSuccess"] = "Reservation created successfully!";
        return RedirectToAction("ReservationSuccess");
    }
    // (EN) Gets the duration based on the type of reservation  | (BG) Връща продължителността на резервацията в зависимост от типа
    private TimeSpan GetDuration(string type) => type switch
    {
        "Extended" => TimeSpan.FromHours(3),
        "ExtendedPlus" => TimeSpan.FromHours(6),
        _ => TimeSpan.FromMinutes(90)
    };
    // (EN) Cancels a reservation  | (BG) Отменя резервация
    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null || reservation.Status != "pending")
        {
            return RedirectToAction("MyReservations");
        }

        reservation.Status = "cancelled";
        await _context.SaveChangesAsync();

        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
        {
            var now = DateTime.Now;

            var reservationsToday = await _context.Reservations
                .Where(r => r.UserId == userId && r.ReservationTime.Date == now.Date)
                .ToListAsync();

            var hasValid = reservationsToday
                .Select(r => new
                {
                    Start = r.ReservationTime.AddMinutes(-30),
                    End = r.ReservationTime.Add(r.DurationType switch
                    {
                        "Extended" => TimeSpan.FromHours(3),
                        "ExtendedPlus" => TimeSpan.FromHours(6),
                        _ => TimeSpan.FromMinutes(90)
                    })
                })
                .Any(x => now >= x.Start && now <= x.End);

            if (!hasValid)
            {
                HttpContext.Session.Remove("Cart");
                HttpContext.Session.SetString("CartStatus", "Disabled");
            }
        }

        TempData["ToastSuccess"] = "Reservation cancelled successfully.";
        return RedirectToAction("MyReservations");
    }
}
