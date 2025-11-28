namespace Order_Project.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Product { get; set; }
        public int Quantity { get; set; }
        public bool IsPaid { get; set; }
        public string Priority { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public string State { get; set; }

    }
}