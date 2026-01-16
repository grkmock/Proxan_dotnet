using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace ProxanReservation.Models;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int AvailableCapacity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Bir etkinliğin birden fazla rezervasyonu olabilir.
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}