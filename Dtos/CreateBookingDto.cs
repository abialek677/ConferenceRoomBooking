using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomBooking.Dtos
{
    public class CreateBookingDto
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }
    }
}
