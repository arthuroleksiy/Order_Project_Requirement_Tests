namespace Order_Project.Services.Intefraces
{
    public interface IDiscountService
    {
        decimal ValidateCode(string discountCode);
    }
}