using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Manager.Data;
using Restaurant_Manager.Models;
using Restaurant_Manager.ViewModels;
using Restaurant_Manager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurant_Manager.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return RedirectToAction("Login", "Auth");

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
                return RedirectToAction("CustomerCart", "Cart");

            var totalPrice = cart.Sum(i => i.Price * i.Quantity);

            var order = new Order
            {
                UserId = userId,
                TotalPrice = totalPrice,
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                OrderItems = cart.Select(i => new OrderItem
                {
                    MenuItemId = i.MenuItemId,
                    Quantity = i.Quantity,
                    Subtotal = i.Price * i.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");

            return RedirectToAction("MyOrders");
        }

        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return RedirectToAction("Login", "Auth");

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var model = new CustomerOrdersViewModel
            {
                Pending = orders.Where(o => o.Status == "pending").ToList(),
                Preparing = orders.Where(o => o.Status == "preparing").ToList(),
                Served = orders.Where(o => o.Status == "served").ToList(),
                Completed = orders.Where(o => o.Status == "completed").ToList(),
                Cancelled = orders.Where(o => o.Status == "cancelled").ToList(),
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> OrderSuccess()
        {
            if (!TempData.ContainsKey("LastOrderId") || !int.TryParse(TempData["LastOrderId"]?.ToString(), out int orderId))
            {
                return RedirectToAction("CustomerCart", "Cart");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return RedirectToAction("CustomerCart", "Cart");
            }
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null && order.Status == "pending")
            {
                order.Status = "cancelled";
                await _context.SaveChangesAsync();
            }
            TempData["ToastSuccess"] = "Order cancelled successfully!";
            return RedirectToAction("MyOrders");
        }


    }
}
