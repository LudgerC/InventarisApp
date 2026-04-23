using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventarisApp.Models
{
    public class Locatie
    {
        public int ID { get; set; }
        
        [Required(ErrorMessage = "Campusnaam is verplicht")]
        [MaxLength(100)]
        public string Naam { get; set; }

        [MaxLength(10)]
        public string? Afkorting { get; set; }
        
        public ICollection<Lokaal> Lokalen { get; set; } = new List<Lokaal>();
    }
}
