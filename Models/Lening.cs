using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarisApp.Models
{
    public class Lening
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int persoonID { get; set; }

        [Required]
        public int DeviceId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime startdatum { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? einddatum { get; set; }

        // Navigation properties voor Foreign Keys
        [ForeignKey("persoonID")]
        public Persoon? Persoon { get; set; }

        [ForeignKey("DeviceId")]
        public Device? Device { get; set; }
    }
}
