using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Restaurant_Manager.Data;
using System.Globalization;
using Restaurant_Manager.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=restaurant.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.Secure = CookieSecurePolicy.Always;

});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Restricted";
    });

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHostedService<ReservationCleanupService>();
builder.Services.AddHttpContextAccessor();

var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();

app.UseCookiePolicy();

app.UseRouting();
app.UseSession(); 
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var userRole = context.Session.GetString("Role");

    if ((path.StartsWithSegments("/Admin") || path.StartsWithSegments("/Reports") || path.StartsWithSegments("/Staff")) && string.IsNullOrEmpty(userRole))
    {
        context.Response.Redirect("/Auth/Login");
        return;
    }

    if ((path.StartsWithSegments("/Admin") || path.StartsWithSegments("/Reports")) && userRole != "admin")
    {
        context.Response.Redirect("/Auth/Restricted");
        return;
    }

    if (path.StartsWithSegments("/Staff") && userRole != "staff")
    {
        context.Response.Redirect("/Auth/Restricted");
        return;
    }

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


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
