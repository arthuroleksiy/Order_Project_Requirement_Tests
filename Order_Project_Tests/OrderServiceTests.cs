using Moq;
using Order_Project.Models;
using Order_Project.Services;
using Order_Project.Services.Intefraces;

namespace Order_Project_Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IInventoryService> inv;
        private readonly Mock<IPaymentService> pay;
        private readonly Mock<INotificationService> note;
        private readonly Mock<IDiscountService> disc;
        private readonly OrderService service;

        public OrderServiceTests()
        {
            inv = new Mock<IInventoryService>();
            pay = new Mock<IPaymentService>();
            note = new Mock<INotificationService>();
            disc = new Mock<IDiscountService>();

            service = new OrderService(inv.Object, pay.Object, note.Object, disc.Object);
        }
    }

}
