using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Restaurant_Manager.Data;
using System.Globalization;
using Restaurant_Manager.Services;

var builder = WebApplication.CreateBuilder(args);

// (EN) Configures the SQLite database connection | (BG) Настройва връзката с базата данни SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=restaurant.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// (EN) Sets strict cookie policy and security | (BG) Задава строгата политика за бисквитки и сигурност
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = builder.Environment.IsDevelopment()
        ? SameSiteMode.Lax
        : SameSiteMode.Strict;
    options.Secure = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

// (EN) Adds session management with specified timeout and secure settings | (BG) Добавя управление на сесиите с указаното време на изтичане и сигурни настройки
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  // (EN) Set session timeout | (BG) Задава време за изтичане на сесията
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = builder.Environment.IsDevelopment()
        ? SameSiteMode.Lax
        : SameSiteMode.Strict;
});

// (EN) Adds authentication with cookie-based authentication | (BG) Добавя автентикация с базирана на бисквитки автентикация
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.Cookie.SameSite = builder.Environment.IsDevelopment()
            ? SameSiteMode.Lax
            : SameSiteMode.Strict;
        options.LoginPath = "/Auth/Login";  // (EN) Path for login | (BG) Път за вход
        options.AccessDeniedPath = "/Auth/Restricted";  // (EN) Path for access denied | (BG) Път за отказан достъп
    });

// (EN) Adds MVC and Razor Pages services | (BG) Добавя услуги за MVC и Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// (EN) Adds a background service for reservation cleanup | (BG) Добавя фоново обслужване за почистване на резервации
builder.Services.AddHostedService<ReservationCleanupService>();

// (EN) Adds HTTP context accessor for session management | (BG) Добавя HTTP контекст за управление на сесиите
builder.Services.AddHttpContextAccessor();

// (EN) Set default culture for the application | (BG) Настройва стандартната култура за приложението
var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var app = builder.Build();

// (EN) Applies database migrations | (BG) Прилага миграции на базата данни
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");  // (EN) Error handler for production | (BG) Обработчик на грешки за продукция
    app.UseHsts();  // (EN) HTTP Strict Transport Security | (BG) Строга транспортна сигурност HTTP
    app.UseHttpsRedirection();  // (EN) Redirects to HTTPS | (BG) Пренасочва към HTTPS
}
else
{
    app.UseDeveloperExceptionPage();  // (EN) Developer exception page for development | (BG) Страница за грешки за разработчици
}

app.UseStaticFiles();  // (EN) Serves static files | (BG) Предоставя статични файлове

app.UseCookiePolicy();  // (EN) Applies cookie policy | (BG) Прилага политика за бисквитки

app.UseRouting();  // (EN) Enables routing | (BG) Активира маршрутизация

app.UseSession();  // (EN) Enables session management | (BG) Активира управление на сесиите
app.UseAuthentication();  // (EN) Enables authentication | (BG) Активира автентикация
app.UseAuthorization();  // (EN) Enables authorization | (BG) Активира авторизация

// (EN) Middleware for user role-based access control | (BG) Middleware за контрол на достъпа според ролята на потребителя
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var userRole = context.Session.GetString("Role");

    // (EN) Redirects to login if not authenticated and accessing restricted paths | (BG) Пренасочва към вход, ако не е влязъл и достъпва ограничени пътеки
    if ((path.StartsWithSegments("/Admin") || path.StartsWithSegments("/Reports") || path.StartsWithSegments("/Staff")) && string.IsNullOrEmpty(userRole))
    {
        context.Response.Redirect("/Auth/Login");
        return;
    }

    // (EN) Redirects to restricted if user role is not admin for admin and report paths |
    // (BG) Пренасочва към ограничен достъп, ако ролята не е администратор за пътища за администратор и доклади
    if ((path.StartsWithSegments("/Admin") || path.StartsWithSegments("/Reports")) && userRole != "admin")
    {
        context.Response.Redirect("/Auth/Restricted");
        return;
    }

    // (EN) Redirects to restricted if user role is not staff for staff paths |
    // (BG) Пренасочва към ограничен достъп, ако ролята не е персонал за пътища за персонал
    if (path.StartsWithSegments("/Staff") && userRole != "staff")
    {
        context.Response.Redirect("/Auth/Restricted");
        return;
    }

    // (EN) Redirects to restricted if user role is not customer for customer-related paths |
    // (BG) Пренасочва към ограничен достъп, ако ролята не е клиент за пътища свързани с клиентите
    if ((path.StartsWithSegments("/Customer") ||
         path.StartsWithSegments("/Account") ||
         path.StartsWithSegments("/Reservation") ||
         path.StartsWithSegments("/Order") ||
         path.StartsWithSegments("/Cart")) && userRole != "customer")
    {
        context.Response.Redirect("/Auth/Restricted");
        return;
    }

    await next();
});

// (EN) Default route configuration | (BG) Стандартна конфигурация на маршрута
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();  // (EN) Runs the application | (BG) Стартира приложението
