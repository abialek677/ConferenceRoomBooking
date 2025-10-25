using ConferenceRoomBooking.Models;

namespace ConferenceRoomBooking.Models.ViewModels
{
    public class RoomManageViewModel
    {
        public List<Room> Rooms { get; set; } = new List<Room>();
        public string? NewRoomName { get; set; }
        public int? NewRoomCapacity { get; set; }
    }
}
