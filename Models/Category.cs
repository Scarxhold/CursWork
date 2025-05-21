using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CursWork.Models
{
    public class Category
    {
        [Key]
        public int Code { get; set; }
        public string Name { get; set; }
        public List<Vehicle> Vehicles { get; set; }
    }
}