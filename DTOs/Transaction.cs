namespace CursWork.DTOs
{
    public class CreateTransactionRequest
    {
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; } 
        public string? CustomerPhone { get; set; } 
        public int? CustomerCode { get; set; } 
        public int? EmployeeCode { get; set; } 
        public int VehicleCode { get; set; }
        public decimal Price { get; set; }
        public string? TransactionDate { get; set; }
    }
}