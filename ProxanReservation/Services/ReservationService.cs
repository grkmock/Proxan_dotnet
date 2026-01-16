using Microsoft.EntityFrameworkCore;
using ProxanReservation.Data;
using ProxanReservation.Models;

namespace ProxanReservation.Services;

public class ReservationService
{
    private readonly AppDbContext _context;
    private const int HoldTimeMinutes = 5;

    public ReservationService(AppDbContext context) => _context = context;

    public async Task<Reservation?> CreateHoldAsync(int eventId, int userId)
    {
        // 1. ADIM: İşlemi (Transaction) başlatıyoruz.
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 2. ADIM: PESSIMISTIC LOCKING (Kötümser Kilitleme)
            // PostgreSQL'de 'FOR UPDATE' bu satırı kilitler. 
            // Aynı anda gelen diğer thread'ler bu satırı okumak için bu işlemin bitmesini bekler.
            var eventItem = await _context.Events
                .FromSqlRaw("SELECT * FROM events WHERE id = {0} FOR UPDATE", eventId)
                .FirstOrDefaultAsync();

            if (eventItem == null || !eventItem.IsActive) return null;

            // -------------------------------------------------------------------------
            // ÖNEMLİ NOT: MÜKERRER KAYIT KONTROLÜ (Duplicate Check)
            // Eğer iş kuralı "Bir kullanıcı sadece 1 bilet alabilir" şeklinde değişirse,/şu an öyle bir durum yok bu nedenle aynı kullanıcı birden fazla rezervasyonu hold yapabiliyor
            // aşağıdaki kodu aktif ederek thread-safe bir kontrol sağlayabiliriz:
            /*
            var alreadyHasRes = await _context.Reservations
                .AnyAsync(r => r.EventId == eventId && r.UserId == userId && 
                               (r.State == ReservationState.Hold || r.State == ReservationState.Confirmed));
            if (alreadyHasRes) return null; 
            */
            // -------------------------------------------------------------------------

            // 3. ADIM: ATOMİK TEMİZLİK (Lazy Cleanup)
            // Kilit altındayken süresi dolanları temizlemek %100 güvenlidir.
            var expiredReservations = await _context.Reservations
                .Where(r => r.EventId == eventId && 
                            r.State == ReservationState.Hold && 
                            r.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            if (expiredReservations.Any())
            {
                int count = expiredReservations.Count;
                eventItem.AvailableCapacity += count; // Kapasiteyi anında iade et
                _context.Reservations.RemoveRange(expiredReservations);
                await _context.SaveChangesAsync();
                Console.WriteLine($"[DEBUG] {count} adet eski kayıt temizlendi. Yeni Kapasite: {eventItem.AvailableCapacity}");
            }

            // 4. ADIM: GÜNCEL KAPASİTE KONTROLÜ
            if (eventItem.AvailableCapacity <= 0)
            {
                return null; // Gerçekten yer kalmadı
            }

            // 5. ADIM: REZERVASYON OLUŞTURMA
            eventItem.AvailableCapacity--;

            var res = new Reservation
            {
                EventId = eventId,
                UserId = userId,
                State = ReservationState.Hold,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(HoldTimeMinutes)
            };

            _context.Reservations.Add(res);
            await _context.SaveChangesAsync();

            // 6. ADIM: ONAY
            await transaction.CommitAsync();
            return res;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"[CRITICAL ERROR] Hold işlemi başarısız: {ex.Message}");
            return null;
        }
    }

    public async Task<(bool success, int? userId)> ConfirmReservationAsync(int reservationId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var reservation = await _context.Reservations
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null || reservation.State != ReservationState.Hold)
                return (false, null);

            if (DateTime.UtcNow > reservation.ExpiresAt)
            {
                if (reservation.Event != null) reservation.Event.AvailableCapacity++;
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (false, null);
            }

            reservation.State = ReservationState.Confirmed;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, reservation.UserId);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return (false, null);
        }
    }
}