using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant_Manager.Data;
using Restaurant_Manager.ViewModels;
using System.Text;
using System.Security.Cryptography;
using Restaurant_Manager.Utils;

namespace Restaurant_Manager.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }
        // (EN) Loads the customer account page | (BG) Зарежда страницата на клиентския акаунт
        public IActionResult CustomerAccount()
        {
            return View();
        }
        // (EN) Loads the page for editing profile | (BG) Зарежда страницата за редактиране на профила
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return RedirectToAction("Login", "Auth");

            var model = new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone
            };

            return View(model);
        }
        // (EN) Handles the edit profile form submission | (BG) Обработва изпращането на формата за редактиране на профила
        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the highlighted fields.";
                return View(model);
            }

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return RedirectToAction("Login", "Auth");

            var user = await _context.Users.AsTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return RedirectToAction("Login", "Auth");

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.Phone = model.Phone;

            await _context.SaveChangesAsync();

            TempData["ToastSuccess"] = "Profile updated successfully.";
            return RedirectToAction("CustomerAccount");
        }
        // (EN) Loads the privacy policy page | (BG) Зарежда страницата с политиката за поверителност
        public IActionResult PrivacyPolicy()
        {
            return View();
        }
        // (EN) Loads the page for changing password | (BG) Зарежда страницата за смяна на паролата
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        // (EN) Handles the change password form submission | (BG) Обработва изпращането на формата за смяна на паролата
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the highlighted fields.";
                return View(model);
            }

            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return RedirectToAction("Login", "Auth");

            using var sha256 = SHA256.Create();
            var oldHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(model.OldPassword)));

            if (user.PasswordHash != oldHash)
            {
                TempData["ToastError"] = "Current password is incorrect.";
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["ToastError"] = "New passwords do not match.";
                return View(model);
            }

            user.PasswordHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(model.NewPassword)));
            await _context.SaveChangesAsync();

            TempData["ToastSuccess"] = "Password changed successfully.";
            return RedirectToAction("CustomerAccount");
        }
    }
}
