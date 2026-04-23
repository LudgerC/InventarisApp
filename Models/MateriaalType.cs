using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventarisApp.Models
{
    public class MateriaalType
    {
        public int ID { get; set; }
        
        [Required(ErrorMessage = "Naam is verplicht")]
        [MaxLength(100)]
        public string Naam { get; set; }
        
        // Navigation property
        public ICollection<Materiaal> Materialen { get; set; } = new List<Materiaal>();
    }
}
