using ConferenceRoomBooking.ConstantValues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ConferenceRoomBooking.Data;
using ConferenceRoomBooking.Models;
using ConferenceRoomBooking.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomBooking.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoomController(ApplicationDbContext context) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var rooms = await context.Rooms.OrderBy(r => r.Name).ToListAsync();
            var viewModel = new RoomManageViewModel
            {
                Rooms = rooms
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddRoom(RoomManageViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NewRoomName) || !model.NewRoomCapacity.HasValue)
            {
                TempData["Error"] = "Nazwa i pojemność salki są wymagane.";
                return RedirectToAction("Manage");
            }
            
            var exists = await context.Rooms.AnyAsync(r => r.Name == model.NewRoomName);
            if (exists)
            {
                TempData["Error"] = "Salka o takiej nazwie już istnieje.";
                return RedirectToAction("Manage");
            }

            var room = new Room
            {
                Name = model.NewRoomName,
                Capacity = model.NewRoomCapacity.Value
            };

            context.Rooms.Add(room);
            await context.SaveChangesAsync();

            TempData["Success"] = "Salka została dodana.";
            return RedirectToAction("Manage");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await context.Rooms.FindAsync(id);
            if (room == null)
            {
                TempData["Error"] = "Salka nie istnieje.";
                return RedirectToAction("Manage");
            }

            context.Rooms.Remove(room);
            await context.SaveChangesAsync();

            TempData["Success"] = "Salka została usunięta.";
            return RedirectToAction("Manage");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Init()
        {
            context.Bookings.RemoveRange(context.Bookings);
            context.Rooms.RemoveRange(context.Rooms);
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();
            
            var users = new List<User>
            {
                new User { Login = "admin", FullName = "Administrator", Role = UserRole.Admin },
                new User { Login = "jan.kowalski", FullName = "Jan Kowalski", Role = UserRole.User },
                new User { Login = "anna.nowak", FullName = "Anna Nowak", Role = UserRole.User },
                new User { Login = "piotr.wisniewski", FullName = "Piotr Wiśniewski", Role = UserRole.User }
            };

            context.Users.AddRange(users);
            await context.SaveChangesAsync();
            
            var rooms = new List<Room>
            {
                new Room { Name = "Kreatywna", Capacity = 10 },
                new Room { Name = "Innowacyjna", Capacity = 6 },
                new Room { Name = "Strategiczna", Capacity = 20 },
                new Room { Name = "Komfortowa", Capacity = 8 }
            };

            context.Rooms.AddRange(rooms);
            await context.SaveChangesAsync();

            TempData["Success"] = "Dane inicjalizacyjne zostały utworzone. Użytkownicy: admin, jan.kowalski, anna.nowak, piotr.wisniewski";
            return RedirectToAction("Login", "Account");
        }
    }
}
