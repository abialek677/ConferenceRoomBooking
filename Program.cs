using ConferenceRoomBooking.ConstantValues;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using ConferenceRoomBooking.Data;
using ConferenceRoomBooking.Models;

var builder = WebApplication.CreateBuilder(args);

// Add MVC and views
builder.Services.AddControllersWithViews();

// Entity Framework with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository as singleton (thread safety)
builder.Services.AddSingleton<BookingRepository>();

// Cookie-based authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", context => {
    context.Response.Redirect("/Account/Login");
    return Task.CompletedTask;
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}");

// Ensure database is created if not present
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    
    if (!context.Users.Any(u => u.Role == UserRole.Admin))
    {
        var admin = new User { Login = "admin", FullName = "Administrator", Role = UserRole.Admin };
        context.Users.Add(admin);
        context.SaveChanges();
    }
}

app.Run();