using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ConferenceRoomBooking.ConstantValues;
using ConferenceRoomBooking.Data;
using ConferenceRoomBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomBooking.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(string login, string fullName)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                ViewBag.Error = "Login jest wymagany.";
                return View();
            }
            if (_context.Users.Any(u => u.Login == login))
            {
                ViewBag.Error = "Ten login jest już zajęty!";
                return View();
            }

            var user = new User
            {
                Login = login,
                FullName = fullName ?? "",
                Role = UserRole.User
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            ViewBag.Success = $"Użytkownik {login} został zarejestrowany. Możesz się zalogować.";
            return View();
        }
        
        [AllowAnonymous]
        [HttpGet("Account/Login/{login?}")]
        public async Task<IActionResult> Login(string? login)
        {
            if (string.IsNullOrEmpty(login))
            {
                return View();
            }
            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);

            if (user == null)
            {
                ViewBag.Error = "Użytkownik nie istnieje.";
                return View();
            }
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Calendar", "Booking");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
