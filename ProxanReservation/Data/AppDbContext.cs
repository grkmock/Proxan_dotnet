using Microsoft.EntityFrameworkCore;
using ProxanReservation.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion; // Gerekli
using System.Linq;

namespace ProxanReservation.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- EVENTS ---
        modelBuilder.Entity<Event>().ToTable("events");
        modelBuilder.Entity<Event>(entity => {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.AvailableCapacity).HasColumnName("available_capacity");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
        });

        // --- USERS ---
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<User>(entity => {
            entity.Property(u => u.Id).HasColumnName("id");
            entity.Property(u => u.Username).HasColumnName("username");
            entity.Property(u => u.Email).HasColumnName("email");
        });

        // --- RESERVATIONS ---
        modelBuilder.Entity<Reservation>().ToTable("reservations");
        modelBuilder.Entity<Reservation>(entity => {
            entity.Property(r => r.Id).HasColumnName("id");
            entity.Property(r => r.EventId).HasColumnName("event_id");
            entity.Property(r => r.UserId).HasColumnName("user_id");
            
            // KRİTİK DÜZELTME: Enum -> String Dönüşümü (InvalidCastException çözümü)
            entity.Property(r => r.State)
                  .HasColumnName("state")
                  .HasConversion<string>(); 

            entity.Property(r => r.CreatedAt).HasColumnName("created_at"); 
            entity.Property(r => r.ExpiresAt).HasColumnName("expires_at");
        });

        // --- UTC CONVERSION (Lambda Hatasız Versiyon) ---
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v, 
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var properties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

            foreach (var property in properties)
            {
                modelBuilder.Entity(entityType.ClrType)
                            .Property(property.Name)
                            .HasConversion(dateTimeConverter);
            }
        }
    }
}