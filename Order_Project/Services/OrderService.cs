using Order_Project.Models;
using Order_Project.Services.Intefraces;

namespace Order_Project.Services
{
    public class OrderService
    {
        private readonly IInventoryService _inventory;
        private readonly IPaymentService _payment;
        private readonly INotificationService _notification;
        private readonly IDiscountService _discounts;
        private readonly List<Order> _orders = new();
        private readonly List<Order> _auditLog = new();

        public OrderService(
            IInventoryService inventory,
            IPaymentService payment,
            INotificationService notification,
            IDiscountService discounts)
        {
            _inventory = inventory;
            _payment = payment;
            _notification = notification;
            _discounts = discounts;
        }

        public Order CreateOrder(string product, int quantity, decimal unitPrice, string priority = "Normal", string discountCode = null)
        {
            ValidateBasicInput(product, quantity);
            ValidateQuantityLimit(quantity);
            ValidatePriority(priority);

            decimal subtotal = quantity * unitPrice;

          
            decimal discount = 0;
            if (!string.IsNullOrEmpty(discountCode))
            {
                discount = _discounts.ValidateCode(discountCode);
                if (discount > subtotal * 0.30m) 
                    throw new InvalidOperationException("Discount too large.");
            }

            decimal finalPrice = subtotal - discount;
            finalPrice += finalPrice * 0.20m; 

            if (priority == "High")
            {
                if (!_inventory.ReserveStock(product, quantity))
                    throw new InvalidOperationException("Cannot reserve stock for high priority order.");
            }
            else
            {
                if (!_inventory.CheckStock(product, quantity))
                    throw new InvalidOperationException("Not enough stock.");
            }

            var order = new Order
            {
                Id = _orders.Count + 1,
                Product = product,
                Quantity = quantity,
                Priority = priority,
                Subtotal = subtotal,
                Discount = discount,
                TotalPrice = finalPrice,
                CreatedAt = DateTime.UtcNow
            };

            if (_payment.NeedsManualApproval(order))
            {
                order.State = "PendingApproval"; 
                _orders.Add(order);
                _notification.SendPendingApproval(order); 
                return order;
            }

            // Process payment normally
            bool paid = _payment.ProcessPayment(order);
            if (!paid)
            {
                if (priority == "High")
                    _inventory.ReleaseReservedStock(product, quantity); 

                throw new InvalidOperationException("Payment failed.");
            }

            order.State = "Paid";
            _orders.Add(order); 
            _notification.SendPaidConfirmation(order); 
            return order;
        }

        private void ValidateBasicInput(string product, int quantity)
        {
            if (string.IsNullOrWhiteSpace(product))
                throw new ArgumentException("Product required.");

            if (product.Length < 3) 
                throw new ArgumentException("Product name too short.");

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive.");
        }

        private void ValidateQuantityLimit(int quantity)
        {
            if (quantity > 100) 
                throw new ArgumentException("Quantity exceeds max limit.");
        }

        private void ValidatePriority(string priority)
        {
            var allowed = new[] { "Low", "Normal", "High" };
            if (!allowed.Contains(priority)) 
                throw new ArgumentException("Invalid priority.");
        }

        public bool UpdateOrder(int id, int newQuantity)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            if (order == null || newQuantity <= 0)
                return false;

            if ((DateTime.UtcNow - order.CreatedAt).TotalDays > 30) 
                return false;

            order.Quantity = newQuantity;
            return true;
        }

        public bool CancelOrder(int id)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
                return false;

            if (order.State == "Shipped") 
                return false;

            order.State = "Cancelled";
            _auditLog.Add(order); 
            _notification.SendCancellation(order); 

            _orders.Remove(order);
            return true;
        }

        public List<Order> GetOrders() => _orders;
        public List<Order> GetAuditLog() => _auditLog;
    }
}
