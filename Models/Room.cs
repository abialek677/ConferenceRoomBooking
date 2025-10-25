using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomBooking.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000)]
        public int Capacity { get; set; }
        
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
