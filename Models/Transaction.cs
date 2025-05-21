using CursWork.Models;
using System.ComponentModel.DataAnnotations;

public class Transaction
{
    [Key]
    public int Code { get; set; }
    [Required]
    public int Vehicle_Code { get; set; }
    [Required]
    public int Customer_Code { get; set; }
    [Required]
    public int? Employee_Code { get; set; }
    [Required]
    public DateTime TransactionDate { get; set; }
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
    public string CustomerName { get; set; } 
    public string CustomerEmail { get; set; } 
    public string CustomerPhone { get; set; } 
    public string VehicleName { get; set; } 
    public Vehicle? Vehicle { get; set; }
    public Employee? Employee { get; set; }
    public Customer? Customer { get; set; }
}