using Microsoft.AspNetCore.Mvc;
using Restaurant_Manager.Data;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

public class AuthController : Controller
{
    private readonly ApplicationDbContext _context;

    public AuthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ToastError"] = "Please correct the highlighted fields.";
            return View(model);
        }

        var errors = new List<string>();

        if (_context.Users.Any(u => u.Email == model.Email))
        {
            errors.Add("Email already exists.");
        }

        if (_context.Users.Any(u => u.Username == model.Username))
        {
            errors.Add("Username already exists.");
        }

        if (_context.Users.Any(u => u.Phone == model.Phone))
        {
            errors.Add("Phone number already exists.");
        }

        if (errors.Any())
        {
            TempData["ToastError"] = string.Join(" ", errors);
            return View(model);
        }

        var user = new User
        {
            Username = model.Username,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Phone = model.Phone,
            Role = "customer"
        };

        user.SetPassword(model.Password);

        _context.Users.Add(user);
        _context.SaveChanges();

        TempData["ToastSuccess"] = "Registration successful! Please login.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            TempData["ToastError"] = "Please fill in all required fields.";
            return View(model);
        }

        var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);
        if (user == null || !user.VerifyPassword(model.Password))
        {
            TempData["ToastError"] = "Invalid username or password.";
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("Username", user.Username);
        HttpContext.Session.SetString("Role", user.Role);

        TempData["ToastSuccess"] = "Logged in successfully.";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return user.Role switch
        {
            "admin" => RedirectToAction("AdminDash", "Admin"),
            "staff" => RedirectToAction("StaffDash", "Staff"),
            "customer" => RedirectToAction("Index", "Customer"),
            _ => RedirectToAction("Restricted", "Auth")
        };
    }

    [HttpGet("check-session")]
    public IActionResult CheckSession()
    {
        var userId = HttpContext.Session.GetString("UserId");
        var username = HttpContext.Session.GetString("Username");
        var role = HttpContext.Session.GetString("Role");

        return Ok(new
        {
            UserId = userId,
            Username = username,
            Role = role
        });
    }

    public IActionResult Restricted()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        TempData["ToastSuccess"] = "Logged out successfully!";
        return RedirectToAction("Login", "Auth");
    }
}
