using PizzaShop.Entity.Data;

namespace PizzaShop.Repository.Interfaces;

public interface IFeedbackRepository
{
    public Task<bool> AddFeedBack(Feedback feedback);
}
