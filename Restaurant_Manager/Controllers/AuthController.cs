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
            return View(model);
        }

        if (_context.Users.Any(u => u.Email == model.Email || u.Username == model.Username))
        {
            ModelState.AddModelError("", "Username or Email already exists.");
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
            return View(model);

        var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);
        if (user == null || !user.VerifyPassword(model.Password))
        {
            ModelState.AddModelError("", "Invalid username or password.");
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

        Console.WriteLine($"[LOGIN DEBUG] User: {user.Username}, Role: {user.Role}");

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

        Console.WriteLine($"Session Data -> UserId: {userId}, Username: {username}, Role: {role}");

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

        return RedirectToAction("Login", "Auth");
    }

}
