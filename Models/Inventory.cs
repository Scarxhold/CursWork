using System.ComponentModel.DataAnnotations;

namespace CursWork.Models
{
    public class Inventory
    {
        [Key]
        public int Code { get; set; }
        public int Vehicle_Code { get; set; }
        public int Manufacturer_Code { get; set; }
        public int Stock_Quantity { get; set; }
        public Vehicle Vehicle { get; set; }
    }
}