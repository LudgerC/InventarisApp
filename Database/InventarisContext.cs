using InventarisApp.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarisApp.Database
{
    public class InventarisContext : DbContext
    {
        public InventarisContext(DbContextOptions<InventarisContext> options) : base(options)
        {
        }

        public DbSet<Device> Devices { get; set; }
        public DbSet<Info> Infos { get; set; }
        public DbSet<Wifi> Wifis { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
