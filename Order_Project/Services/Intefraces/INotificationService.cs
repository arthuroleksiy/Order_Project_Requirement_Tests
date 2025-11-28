using Order_Project.Models;

namespace Order_Project.Services.Intefraces
{
    public interface INotificationService
    {
        void SendCancellation(Order order);
        void SendConfirmation(Order order);
        void SendPaidConfirmation(Order order);
        void SendPaymentFailed(Order order);
        void SendPendingApproval(Order order);
    }
}
