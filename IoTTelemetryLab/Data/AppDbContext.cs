using IoTTelemetryLab.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IoTTelemetryLab.Data;

public class AppDbContext : DbContext
{
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<TelemetryRecord> Records => Set<TelemetryRecord>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "iot_telemetry.db");

        options.UseSqlite($"Data Source={dbPath}")
               .LogTo(SqlCommandCounter.OnLog,
                      new[] { DbLoggerCategory.Database.Command.Name },
                      LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>()
            .Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<TelemetryRecord>()
            .HasOne(r => r.Device)
            .WithMany(d => d.Records)
            .HasForeignKey(r => r.DeviceId);

        modelBuilder.Entity<TelemetryRecord>()
            .Property(r => r.Source)
            .IsRequired()
            .HasMaxLength(20);
    }
}
