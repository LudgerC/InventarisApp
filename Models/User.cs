using System.ComponentModel.DataAnnotations;

namespace InventarisApp.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Personeel"; // Can be 'Admin' or 'Personeel'

        public bool IsActive { get; set; } = true;
    }
}
