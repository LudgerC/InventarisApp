using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarisApp.Models
{
    public class Lokaal
    {
        // Composite Key: locatie_id, lokaalnr
        public int locatie_id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string lokaalnr { get; set; }
        
        public int plaatsen { get; set; }
        
        // Navigation properties
        [ForeignKey("locatie_id")]
        public Locatie? Locatie { get; set; }
        
        public ICollection<Info>? Infos { get; set; } = new List<Info>();
    }
}
