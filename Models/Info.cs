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
        
        // Foreign Key to Lokaal
        public int locatie_id { get; set; }
        [Required]
        [MaxLength(50)]
        public string lokaalnr { get; set; }
        
        // Data properties
        [MaxLength(100)]
        public string merk { get; set; }
        
        [MaxLength(50)]
        public string ip { get; set; }
        
        [MaxLength(100)]
        public string model { get; set; }
        
        [MaxLength(100)]
        public string serial_number { get; set; }
        
        [MaxLength(50)]
        public string status { get; set; }
        
        public int garantie { get; set; }
        
        [MaxLength(100)]
        public string leverancier { get; set; }
        
        public DateTime? aankoopdatum { get; set; }
        
        public DateTime? eind_garantie { get; set; }
        
        // Navigation properties
        [ForeignKey("device_id")]
        public Device Device { get; set; }
        
        [ForeignKey("locatie_id,lokaalnr")]
        public Lokaal Lokaal { get; set; }
        
        public ICollection<Wifi> Wifis { get; set; } = new List<Wifi>();
    }
}
