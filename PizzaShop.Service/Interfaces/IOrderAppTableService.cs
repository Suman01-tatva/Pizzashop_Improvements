using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Service.Interfaces;

public interface IOrderAppTableService
{
    public Task<OrderAppTableViewModel> GetTableView();
    public Task<AssignTablePageViewModel> GetWaitingTokensBySectionId(int sectionId);
    public Task<string> AssignTableToCustomer(AssignTableViewModel model, int userId);

}
