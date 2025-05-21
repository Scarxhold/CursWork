using System.ComponentModel.DataAnnotations;

namespace CursWork.Models
{
    public class Employee
    {
        [Key]
        public int Code { get; set; }
        public string Name { get; set; }
        public int Department_Code { get; set; }
    }
}