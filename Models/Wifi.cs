using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarisApp.Models
{
    public class Wifi
    {
        [Key]
        public int wifi_id { get; set; }
        
        // Foreign Key to Info: type + device_id
        [Required]
        [MaxLength(50)]
        public string type { get; set; }
        
        public int device_id { get; set; }
        
        [MaxLength(100)]
        public string? mac_address { get; set; }
        
        [MaxLength(50)]
        public string? local_ip { get; set; }
        
        // Navigation property
        [ForeignKey("type,device_id")]
        public Info? Info { get; set; }
    }
}
