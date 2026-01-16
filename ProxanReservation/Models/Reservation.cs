using System;

namespace ProxanReservation.Models;

public class Reservation
{
    public int Id { get; set; } // 'public' olduğundan emin ol
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int EventId { get; set; }
    public int UserId { get; set; }
    public ReservationState State { get; set; }
    public DateTime ExpiresAt { get; set; }
    
    // Navigation property - Döngüsel referans hatası almamak için bunu ignore edebilirsin
    [System.Text.Json.Serialization.JsonIgnore]
    public Event? Event { get; set; }
}