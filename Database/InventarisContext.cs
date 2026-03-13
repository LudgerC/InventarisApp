using InventarisApp.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarisApp.Database
{
    public class InventarisContext : DbContext
    {
        public InventarisContext(DbContextOptions<InventarisContext> options) : base(options)
        {
        }

        public DbSet<Locatie> Locaties { get; set; }
        public DbSet<Lokaal> Lokalen { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Info> Infos { get; set; }
        public DbSet<Wifi> Wifis { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Lokaal composite key
            modelBuilder.Entity<Lokaal>()
                .HasKey(l => new { l.locatie_id, l.lokaalnr });

            // Configure Info composite key
            modelBuilder.Entity<Info>()
                .HasKey(i => new { i.type, i.device_id });

            // Ensure properties map to the columns properly
            modelBuilder.Entity<Info>()
                .Property(i => i.serial_number)
                .HasColumnName("serial number");

            modelBuilder.Entity<Info>()
                .Property(i => i.eind_garantie)
                .HasColumnName("eind garantie");
                
            // Define Locatie -> Lokaal relationship
            modelBuilder.Entity<Lokaal>()
                .HasOne(l => l.Locatie)
                .WithMany(loc => loc.Lokalen)
                .HasForeignKey(l => l.locatie_id)
                .OnDelete(DeleteBehavior.Restrict);

            // Define Lokaal -> Info relationship
            modelBuilder.Entity<Info>()
                .HasOne(i => i.Lokaal)
                .WithMany(l => l.Infos)
                .HasForeignKey(i => new { i.locatie_id, i.lokaalnr })
                .OnDelete(DeleteBehavior.Restrict);
                
            // Define Info -> Device relationship (Although device_id is FK)
            modelBuilder.Entity<Info>()
                .HasOne(i => i.Device)
                .WithMany(d => d.Infos)
                .HasForeignKey(i => i.device_id)
                .OnDelete(DeleteBehavior.Cascade);

            // Define Wifi -> Info relationship
            modelBuilder.Entity<Wifi>()
                .HasOne(w => w.Info)
                .WithMany(i => i.Wifis)
                .HasForeignKey(w => new { w.type, w.device_id })
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
