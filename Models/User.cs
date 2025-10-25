using System.ComponentModel.DataAnnotations;
using ConferenceRoomBooking.ConstantValues;

namespace ConferenceRoomBooking.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Login { get; set; } = string.Empty;

        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        public UserRole Role { get; set; } = UserRole.User;
        
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
