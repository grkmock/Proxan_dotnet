using ProxanReservation.Data;
using ProxanReservation.Models;
using Microsoft.EntityFrameworkCore;

namespace ProxanReservation.Services;

public class ExpiredReservationWorker : BackgroundService
{
    private readonly IServiceProvider _services;

    private const int ExpiredReservationControlTime = 10000; // 10 saniyede bir kontrol et

    public ExpiredReservationWorker(IServiceProvider services) => _services = services;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                // Süresi dolmuş ve hala HOLD durumunda olanları bul
                var expiredReservations = await context.Reservations
                    .Include(r => r.Event)
                    .Where(r => r.State == ReservationState.Hold && r.ExpiresAt < DateTime.UtcNow)
                    .ToListAsync();

                foreach (var res in expiredReservations)
                {
                    res.State = ReservationState.Expired;
                    if (res.Event != null) res.Event.AvailableCapacity++;
                    Console.WriteLine($"[Sistem] Rezervasyon {res.Id} süresi dolduğu için iptal edildi. Kapasite iade edildi.");
                }

                if (expiredReservations.Any()) await context.SaveChangesAsync();
            }

            await Task.Delay(ExpiredReservationControlTime, stoppingToken); // 10 saniyede bir kontrol et
        }
    }
}