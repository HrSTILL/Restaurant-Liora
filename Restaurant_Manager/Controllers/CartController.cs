using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Manager.Data;
using Restaurant_Manager.Models;
using Restaurant_Manager.Models.ViewModels;
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
        public async Task<JsonResult> AddToCartAjax([FromBody] AddToCartRequest data)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Json(new { success = false, message = "Not logged in" });

            if (!await IsUserReservationActive(userId))
                return Json(new { success = false, message = "No valid reservation." });

            var now = DateTime.Now;

            var reservationsToday = await _context.Reservations
                .Where(r => r.UserId == userId &&
                            (r.Status == "pending" || r.Status == "confirmed") &&
                            r.ReservationTime.Date == now.Date)
                .ToListAsync();

            var hasValidReservation = reservationsToday
                .Select(r => new
                {
                    Start = r.ReservationTime.AddMinutes(-30),
                    End = r.ReservationTime.Add(GetReservationDuration(r.DurationType))
                })
                .Any(x => now >= x.Start && now <= x.End);

            if (!hasValidReservation)
            {
                HttpContext.Session.Remove("Cart");
                HttpContext.Session.SetString("CartStatus", "Disabled");
                return Json(new { success = false, message = "No valid reservation." });
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
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

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
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
        public async Task<IActionResult> UpdateQuantityAjax([FromBody] UpdateQuantityRequestViewModel request)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId) || !await IsUserReservationActive(userId))
                return Json(new { success = false, message = "No valid reservation." });

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MenuItemId == request.MenuItemId);
            if (item != null && request.Quantity > 0)
            {
                item.Quantity = request.Quantity;
                SaveCart(cart);
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<JsonResult> RemoveFromCartAjax([FromBody] RemoveRequestViewModel data)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId) || !await IsUserReservationActive(userId))
                return Json(new { success = false, message = "No valid reservation." });

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.MenuItemId == data.MenuItemId);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
                return Json(new { success = true, newCount = cart.Sum(i => i.Quantity) });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            TempData["ToastSuccess"] = "Cart cleared successfully!";
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

            if (!await IsUserReservationActive(userId))
            {
                HttpContext.Session.Remove(CartSessionKey);
                TempData["Error"] = "You don’t have an active reservation right now.";
                return RedirectToAction("CustomerCart");
            }

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

            TempData["ToastSuccess"] = "Order placed successfully!";
            return RedirectToAction("OrderSuccess", "Order");
        }

        [HttpGet]
        public async Task<IActionResult> CheckReservation()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Json(new { success = false });

            return Json(new { success = await IsUserReservationActive(userId) });
        }

        private async Task<bool> IsUserReservationActive(int userId)
        {
            var now = DateTime.Now;
            var reservationsToday = await _context.Reservations
                .Where(r => r.UserId == userId &&
                            (r.Status == "pending" || r.Status == "confirmed") &&
                            r.ReservationTime.Date == now.Date)
                .ToListAsync();

            var isActive = reservationsToday
                .Select(r => new
                {
                    Start = r.ReservationTime.AddMinutes(-30),
                    End = r.ReservationTime.Add(GetReservationDuration(r.DurationType))
                })
                .Any(x => now >= x.Start && now <= x.End);

            if (!isActive)
            {
                HttpContext.Session.Remove("Cart");
                HttpContext.Session.SetString("CartStatus", "Disabled");
            }

            return isActive;
        }


        private TimeSpan GetReservationDuration(string type) => type switch
        {
            "Extended" => TimeSpan.FromHours(3),
            "ExtendedPlus" => TimeSpan.FromHours(6),
            _ => TimeSpan.FromMinutes(90)
        };
    }
}
