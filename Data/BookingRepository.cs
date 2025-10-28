using System.Collections.Concurrent;
using ConferenceRoomBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomBooking.Data
{
    public class BookingRepository(IServiceProvider serviceProvider)
    {
        private readonly object _lockObject = new object();

        public async Task<(bool Success, string Message)> CreateBookingAsync(Booking booking)
        {
            lock (_lockObject)
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                if (!booking.IsValid())
                {
                    return (false, "Rezerwacja musi trwać od 15 minut do 3 godzin.");
                }
                
                bool hasConflict = context.Bookings
                    .Any(b => b.RoomId == booking.RoomId &&
                              b.Id != booking.Id &&
                              ((booking.StartTime >= b.StartTime && booking.StartTime < b.EndTime) ||
                               (booking.EndTime > b.StartTime && booking.EndTime <= b.EndTime) ||
                               (booking.StartTime <= b.StartTime && booking.EndTime >= b.EndTime)));

                if (hasConflict)
                {
                    return (false, "Ten termin jest już zajęty.");
                }
                
                context.Bookings.Add(booking);
                context.SaveChanges();

                return (true, "Rezerwacja została utworzona pomyślnie.");
            }
        }

        public async Task<List<Booking>> GetBookingsForDayAsync(DateTime date, int? roomId = null)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var startOfDay = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var endOfDay = startOfDay.AddDays(1);

            var query = context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Where(b => b.StartTime >= startOfDay && b.StartTime < endOfDay);

            if (roomId.HasValue)
            {
                query = query.Where(b => b.RoomId == roomId.Value);
            }

            return await query.OrderBy(b => b.StartTime).ToListAsync();
        }

        public async Task<List<Booking>> GetUserBookingsAsync(int userId)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Where(b => b.UserId == userId && b.StartTime >= DateTime.UtcNow)
                .OrderBy(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<bool> CancelBookingAsync(int bookingId, int userId)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var booking = await context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

            if (booking == null)
            {
                return false;
            }

            context.Bookings.Remove(booking);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Room>> GetAllRoomsAsync()
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            return await context.Rooms.OrderBy(r => r.Name).ToListAsync();
        }
    }
}
