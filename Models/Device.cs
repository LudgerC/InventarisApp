using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventarisApp.Models
{
    public class Device
    {
        [Key]
        public int device_id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string type { get; set; }
        
        // Navigation property
        public ICollection<Info> Infos { get; set; } = new List<Info>();
    }
}
