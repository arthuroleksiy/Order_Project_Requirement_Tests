using Order_Project.Models;

namespace Order_Project.Services.Intefraces
{
    public interface IPaymentService
    {
        bool NeedsManualApproval(Order order);
        bool ProcessPayment(Order order);
    }
}
