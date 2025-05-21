using System;
using System.ComponentModel.DataAnnotations;

namespace CursWork.Models
{
    public class Customer
    {
        [Key]
        public int Code { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Role { get; set; }
    }
}