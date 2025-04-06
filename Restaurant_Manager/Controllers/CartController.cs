using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Manager.Data;
using Restaurant_Manager.Models;
using Restaurant_Manager.Utils;
using Restaurant_Manager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Restaurant_Manager.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "Cart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult CustomerCart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            return View(cart);
        }

        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(json)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            var json = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartSessionKey, json);
        }

        [HttpPost]
        public JsonResult AddToCartAjax([FromBody] AddToCartRequest data)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var item = _context.MenuItems.FirstOrDefault(m => m.Id == data.MenuItemId);
            if (item == null)
                return Json(new { success = false });

            var existing = cart.FirstOrDefault(x => x.MenuItemId == item.Id);
            if (existing != null)
                existing.Quantity++;
            else
                cart.Add(new CartItem
                {
                    MenuItemId = item.Id,
                    Name = item.Name,
                    Price = item.Price,
                    ImageUrl = item.ImageUrl,
                    Quantity = 1,
                    Tags = item.Tags
                });

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return Json(new { success = true, itemCount = cart.Sum(i => i.Quantity) });
        }

        public class AddToCartRequest
        {
            public int MenuItemId { get; set; }
        }


        [HttpPost]
        public IActionResult RemoveFromCart(int menuItemId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MenuItemId == menuItemId);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }
            return RedirectToAction("CustomerCart");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int menuItemId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MenuItemId == menuItemId);
            if (item != null && quantity > 0)
            {
                item.Quantity = quantity;
                SaveCart(cart);
            }
            return RedirectToAction("CustomerCart");
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("CustomerCart");
        }


        [HttpGet]
        public IActionResult PlaceOrder()
        {
            var cart = GetCart();
            if (!cart.Any())
                return RedirectToAction("CustomerCart");

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmOrder()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return RedirectToAction("Login", "Auth");

            var cart = GetCart();
            if (!cart.Any())
                return RedirectToAction("CustomerCart");

            var now = DateTime.Now;

            var reservationsToday = await _context.Reservations
                .Where(r => r.UserId == userId && r.ReservationTime.Date == now.Date)
                .ToListAsync();

            var validReservation = reservationsToday
                .Select(r => new
                {
                    Reservation = r,
                    Start = r.ReservationTime.AddMinutes(-30),
                    End = r.ReservationTime.Add(GetReservationDuration(r.DurationType))
                })
                .Where(x => now >= x.Start && now <= x.End)
                .OrderBy(x => Math.Abs((x.Reservation.ReservationTime - now).Ticks))
                .FirstOrDefault();

            if (validReservation == null)
            {
                TempData["Error"] = "You don’t have an active reservation right now.";
                return RedirectToAction("CustomerCart");
            }

            var reservation = validReservation.Reservation;

            var order = new Order
            {
                UserId = userId,
                TotalPrice = cart.Sum(x => x.Price * x.Quantity),
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                OrderItems = cart.Select(x => new OrderItem
                {
                    MenuItemId = x.MenuItemId,
                    Quantity = x.Quantity,
                    Subtotal = x.Price * x.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove(CartSessionKey);

            TempData["LastOrderId"] = order.Id;
            return RedirectToAction("OrderSuccess", "Order");
        }

        [HttpGet]
        public async Task<IActionResult> CheckReservation()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Json(new { success = false });

            var now = DateTime.Now;
            var today = DateTime.Today;

            var reservationsToday = await _context.Reservations
                .Where(r => r.UserId == userId && r.ReservationTime.Date == today)
                .ToListAsync();

            var isActive = reservationsToday
                .Select(r => new
                {
                    Start = r.ReservationTime.AddMinutes(-30),
                    End = r.ReservationTime.Add(GetReservationDuration(r.DurationType))
                })
                .Any(x => now >= x.Start && now <= x.End);

            return Json(new { success = isActive });
        }

        private TimeSpan GetReservationDuration(string type) => type switch
        {
            "Extended" => TimeSpan.FromHours(3),
            "ExtendedPlus" => TimeSpan.FromHours(6),
            _ => TimeSpan.FromMinutes(90)
        };
    }
}
