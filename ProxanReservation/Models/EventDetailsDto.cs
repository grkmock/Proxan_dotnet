namespace ProxanReservation.Models;

public class EventDetailsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TotalCapacity { get; set; }
    public int RemainingCapacity { get; set; } // Mevcut AvailableCapacity
    public int HoldCount { get; set; }
    public int ConfirmedCount { get; set; }
}