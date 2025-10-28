using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ConferenceRoomBooking.Data;
using ConferenceRoomBooking.Models;
using ConferenceRoomBooking.Models.ViewModels;
using System.Security.Claims;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using System.Text;
using ConferenceRoomBooking.Dtos;

namespace ConferenceRoomBooking.Controllers
{
    [Authorize]
    public class BookingController(BookingRepository repository) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Calendar(DateTime? date)
        {
            var selectedDate = date?.ToUniversalTime() ?? DateTime.UtcNow.Date;
            ViewBag.SelectedDate = selectedDate;
            ViewBag.Rooms = await repository.GetAllRoomsAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetForDay(DateTime date)
        {
            var bookings = await repository.GetBookingsForDayAsync(date);

            var result = bookings.Select(b => new
            {
                id = b.Id,
                roomId = b.RoomId,
                roomName = b.Room.Name,
                userName = b.User.FullName,
                startTime = b.StartTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                endTime = b.EndTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                isOwner = b.UserId.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier)
            });

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookingDto model)
        {
            if (!ModelState.IsValid)
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(" | ", 
                        ModelState.Values.SelectMany(v=>v.Errors.Select(e=>e.ErrorMessage)));
                    return Json(new { success = false, message = "Nieprawidłowe dane: " + errors });
                }
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var booking = new Booking
            {
                UserId = userId,
                RoomId = model.RoomId,
                StartTime = model.StartTime.ToUniversalTime(),
                EndTime = model.EndTime.ToUniversalTime()
            };

            var result = await repository.CreateBookingAsync(booking);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var bookings = await repository.GetUserBookingsAsync(userId);
            return View(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await repository.CancelBookingAsync(id, userId);

            if (success)
            {
                TempData["Success"] = "Rezerwacja została anulowana.";
            }
            else
            {
                TempData["Error"] = "Nie można anulować tej rezerwacji.";
            }

            return RedirectToAction("MyBookings");
        }

        [HttpGet]
        public async Task<IActionResult> ExportMyBookings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var bookings = await repository.GetUserBookingsAsync(userId);

            if (bookings == null || bookings.Count == 0)
            {
                TempData["Error"] = "Brak nadchodzących rezerwacji do eksportu.";
                return RedirectToAction("MyBookings");
            }

            var calendar = new Calendar();
            calendar.AddProperty("PRODID", "-//Conference Room Booking//EN");
            calendar.AddProperty("VERSION", "2.0");

            foreach (var booking in bookings)
            {
                var roomName = booking.Room?.Name ?? "Sala";
                var roomCapacity = booking.Room?.Capacity ?? 0;
                var organizerName = booking.User?.FullName ?? User.Identity?.Name ?? "Użytkownik";
                
                var startUtc = DateTime.SpecifyKind(booking.StartTime, DateTimeKind.Utc);
                var endUtc = DateTime.SpecifyKind(booking.EndTime, DateTimeKind.Utc);

                var calEvent = new CalendarEvent
                {
                    Summary = $"Rezerwacja: {roomName}",
                    Description = $"Salka: {roomName} (pojemność: {roomCapacity})",
                    Start = new CalDateTime(startUtc) { TzId = "UTC" },
                    End = new CalDateTime(endUtc) { TzId = "UTC" },
                    Location = roomName,
                    Uid = $"booking-{booking.Id}@conferenceroombooking.local"
                };

                calEvent.Organizer = new Organizer
                {
                    CommonName = organizerName
                };

                calendar.Events.Add(calEvent);
            }

            var serializer = new CalendarSerializer();
            var icsContent = serializer.SerializeToString(calendar);
            var bytes = Encoding.UTF8.GetBytes(icsContent);
            
            return File(bytes, "text/calendar; charset=utf-8", $"moje-rezerwacje-{DateTime.UtcNow:yyyyMMdd}.ics");
        }
    }
}
