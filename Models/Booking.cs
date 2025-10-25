using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConferenceRoomBooking.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        public int RoomId { get; set; }

        [ForeignKey("RoomId")]
        public Room Room { get; set; } = null!;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsValid()
        {
            var duration = EndTime - StartTime;
            return duration.TotalMinutes >= 15 && duration.TotalHours <= 3 && StartTime < EndTime;
        }
    }
}
