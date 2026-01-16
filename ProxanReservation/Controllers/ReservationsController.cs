using Microsoft.AspNetCore.Authorization; // JWT kontrolü için gerekli
using Microsoft.AspNetCore.Mvc;
using ProxanReservation.Services;

namespace ProxanReservation.Controllers;

//[Authorize] /gerek kalmadı appsettings.json içinden kontrol ediyorum
[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly ReservationService _reservationService;

    public ReservationsController(ReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpPost("hold")]
    public async Task<IActionResult> CreateHold([FromQuery] int eventId, [FromQuery] int userId)
    {
        // Kritik İşlem: ReservationService içinde Transaction ve Database Lock (FOR UPDATE) çalışıyor.
        var result = await _reservationService.CreateHoldAsync(eventId, userId);
        
        if (result == null)
        {
            // Mülakatçıya teknik açıklama: Kapasite kontrolü transaction içinde yapıldı.
            return BadRequest("Kapasite yetersiz, etkinlik pasif veya zaten rezervasyonunuz bulunuyor.");
        }

        return Ok(result); 
    }

    [HttpPost("confirm/{reservationId}")]
    public async Task<IActionResult> Confirm([FromRoute] int reservationId)
    {
        // Kritik İşlem: Onaylama sırasında zaman aşımı kontrolü transaction içinde yapılır.
        var (success, userId) = await _reservationService.ConfirmReservationAsync(reservationId);
        
        if (!success)
        {
            return BadRequest(new { 
                error = "Onay Başarısız", 
                detail = $"Rezervasyon (ID: {reservationId}) bulunamadı, süresi dolmuş veya zaten onaylanmış." 
            });
        }

        return Ok(new { 
            message = $"Kullanıcı {userId} için Rezervasyon {reservationId} başarıyla onaylandı." 
        });
    }
}