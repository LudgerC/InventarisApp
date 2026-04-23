using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarisApp.Models
{
    public class Lokaal
    {
        public int ID { get; set; }
        
        [Required(ErrorMessage = "Lokaalnaam is verplicht")]
        [MaxLength(100)]
        public string Naam { get; set; }
        
        [MaxLength(200)]
        public string? Beschrijving { get; set; }
        
        [Range(0, 1000, ErrorMessage = "Aantal plaatsen moet tussen 0 en 1000 zijn")]
        public int AantalPlaatsen { get; set; }
        
        public bool IsExtern { get; set; }
        
        public int? LocatieId { get; set; }
        
        [ForeignKey("LocatieId")]
        public Locatie? Locatie { get; set; }
        
        // Navigation properties
        public ICollection<Info> Devices { get; set; } = new List<Info>();
        public ICollection<Materiaal> Materialen { get; set; } = new List<Materiaal>();
    }
}
