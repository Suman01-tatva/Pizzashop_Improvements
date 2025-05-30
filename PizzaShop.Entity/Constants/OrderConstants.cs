namespace PizzaShop.Entity.Constants;

public class OrderConstants
{
    public enum OrderStatusEnum
    {
        AllStatus = 0,
        Pending = 1,
        Completed = 2,
        InProgress = 3,
        Running = 4,
        Served = 5,
        Canceled = 6,
        Failed = 7
    }

    public enum PaymentModeEnum
    {
        Pending = 0,
        Online = 1,
        Card = 2,
        Cash = 3
    }

    public enum PaymentStatusEnum
    {
        Completed = 1,
        Pending = 2
    }
}
