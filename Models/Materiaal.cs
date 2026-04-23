using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarisApp.Models
{
    public class Materiaal
    {
        public int ID { get; set; }
        
        [Required]
        public int MateriaalTypeId { get; set; }
        
        [ForeignKey("MateriaalTypeId")]
        public MateriaalType? MateriaalType { get; set; }
        
        [Range(1, 1000)]
        public int Aantal { get; set; } = 1;
        
        public int LokaalId { get; set; }
        
        [ForeignKey("LokaalId")]
        public Lokaal? Lokaal { get; set; }
    }
}
