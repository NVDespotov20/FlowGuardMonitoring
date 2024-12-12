using FlowGuardMonitoring.DAL.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FlowGuardMonitoring.DAL.Data;

public class FlowGuardMonitoringContext : IdentityDbContext<User>
{
    public FlowGuardMonitoringContext(DbContextOptions<FlowGuardMonitoringContext> options)
        : base(options)
    {
    }

    public DbSet<Measurement> Measurements { get; set; }
    public DbSet<Sensor> Sensors { get; set; }
    public DbSet<Site> Sites { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Measurement>()
            .HasKey(w => w.MeasurementId);

        modelBuilder.Entity<Sensor>()
            .HasKey(s => s.SensorId);

        modelBuilder.Entity<Site>()
            .HasKey(s => s.SiteId);

        modelBuilder.Entity<Measurement>()
            .HasOne(m => m.Sensor)
            .WithMany(m => m.Measurements)
            .HasForeignKey(m => m.SensorId);

        modelBuilder.Entity<Sensor>()
            .HasOne(s => s.Site)
            .WithMany(s => s.Sensors)
            .HasForeignKey(s => s.SiteId);

        modelBuilder.Entity<Site>()
            .HasOne(s => s.User)
            .WithMany(u => u.Sites)
            .HasForeignKey(s => s.UserId);
    }
}