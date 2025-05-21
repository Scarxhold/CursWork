using System.ComponentModel.DataAnnotations;

namespace CursWork.DTOs
{
    public class AddVehicleDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        public int Category_Code { get; set; }
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        [Required]
        [Range(1900, 2025)]
        public int Year { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int Mileage { get; set; }
        public string Image { get; set; }
    }
}