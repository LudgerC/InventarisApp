using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventarisApp.Models
{
    public class Persoon
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Naam { get; set; }

        [Required]
        [MaxLength(100)]
        public string Achternaam { get; set; }

        [EmailAddress]
        [MaxLength(150)]
        public string? emailadres { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? tel { get; set; }

        [MaxLength(100)]
        public string? functie { get; set; }

        // Navigation property naar Lening
        public ICollection<Lening> Leningen { get; set; } = new List<Lening>();
    }
}
