namespace CursWork.DTOs
{
    public class VehicleDTO
    {
        public int code { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public string image { get; set; }
        public int category_Code { get; set; }
        public string categoryName { get; set; }
        public int year { get; set; }
        public int mileage { get; set; }
    }
}