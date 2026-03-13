using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventarisApp.Models
{
    public class Locatie
    {
        [Key]
        public int locatie_id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string naam { get; set; }
        
        [MaxLength(10)]
        public string abbreviation { get; set; }
        
        // Navigation property
        public ICollection<Lokaal> Lokalen { get; set; } = new List<Lokaal>();
    }
}
