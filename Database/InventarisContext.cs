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
        public DbSet<User> Users { get; set; }

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

            // Seed Opslag data
            modelBuilder.Entity<Locatie>().HasData(
                new Locatie { locatie_id = 1, naam = "Campus Rouppe", abbreviation = "CR" },
                new Locatie { locatie_id = 2, naam = "Campus Landsroem", abbreviation = "CL" }
            );

            modelBuilder.Entity<Lokaal>().HasData(
                new Lokaal { locatie_id = 1, lokaalnr = "Opslag", plaatsen = 0 },
                new Lokaal { locatie_id = 2, lokaalnr = "Opslag", plaatsen = 0 }
            );

            // Seed Admin data
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    // Hashed "Admin123!" using BCrypt
                    PasswordHash = "$2a$11$eNfFxyOQ/7o76hXwE/18/.M9v2vDkY6m8tP51BwEZyV44Qx74o77G",
                    Role = "Admin",
                    IsActive = true
                }
            );
        }
    }
}
