using System.ComponentModel.DataAnnotations;

namespace CursWork.Models
{
    public class Maintenance
    {
        [Key]
        public int Code { get; set; }
        [Required]
        public int Vehicle_Code { get; set; }
        [Required]
        public DateTime Maintenance_Date { get; set; }
        [Required]
        [StringLength(255)]
        public string Description { get; set; }
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Cost { get; set; }
        public Vehicle Vehicle { get; set; }
    }
}