using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarisApp.Models
{
    public class Info
    {
        // Composite Key: type, device_id. Wait, `type` is in Device, but `type` + `device_id` is the PK here.
        [Required]
        [MaxLength(50)]
        public string type { get; set; }
        
        public int device_id { get; set; }
        
        // Data properties
        [MaxLength(100)]
        public string? merk { get; set; }

        [MaxLength(100)]
        public string? apparaatnaam { get; set; }
        
        [MaxLength(50)]
        [RegularExpression(@"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$", ErrorMessage = "Ongeldig IP-adres.")]
        public string? ip { get; set; }
        
        [MaxLength(100)]
        public string? model { get; set; }
        
        [MaxLength(100)]
        public string? serial_number { get; set; }
        
        [MaxLength(50)]
        public string? status { get; set; }
        
        public int? garantie { get; set; }
        
        [MaxLength(100)]
        public string? leverancier { get; set; }
        
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? aankoopdatum { get; set; }
        
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? eind_garantie { get; set; }
        
        // Navigation properties
        [ForeignKey("device_id")]
        public Device? Device { get; set; }
        
        public int? LokaalId { get; set; }
        
        [ForeignKey("LokaalId")]
        public Lokaal? Lokaal { get; set; }

        public ICollection<Wifi> Wifis { get; set; } = new List<Wifi>();
    }
}
