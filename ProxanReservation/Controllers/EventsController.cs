using Microsoft.AspNetCore.Authorization; // Auth için gerekli
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProxanReservation.Data;
using ProxanReservation.Models;

namespace ProxanReservation.Controllers;

//[Authorize] /gerek kalmadı appsettings.json içinden kontrol ediyorum
[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _context;
    public EventsController(AppDbContext context) => _context = context;

    [HttpPost]
    // Sadece token sahibi olanlar (yönetici/organizatör gibi) etkinlik oluşturabilir
    public async Task<IActionResult> Create([FromBody] Event eventItem)
    {
        if (eventItem == null || string.IsNullOrEmpty(eventItem.Title))
        {
            return BadRequest("Etkinlik verileri eksik veya hatalı formatta.");
        }

        eventItem.AvailableCapacity = eventItem.Capacity;
        _context.Events.Add(eventItem);
        await _context.SaveChangesAsync();
        
        return Ok(eventItem);
    }

    [AllowAnonymous] // Etkinlik detaylarını herkesin görebilmesi için istisna tanıyoruz
    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetEventDetails(int id)
    {
        var eventItem = await _context.Events
            .Include(e => e.Reservations)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (eventItem == null) return NotFound("Etkinlik bulunamadı.");

        var details = new EventDetailsDto
        {
            Id = eventItem.Id,
            Title = eventItem.Title,
            TotalCapacity = eventItem.Capacity,
            RemainingCapacity = eventItem.AvailableCapacity,
            HoldCount = eventItem.Reservations.Count(r => r.State == ReservationState.Hold),
            ConfirmedCount = eventItem.Reservations.Count(r => r.State == ReservationState.Confirmed)
        };

        return Ok(details);
    }
}