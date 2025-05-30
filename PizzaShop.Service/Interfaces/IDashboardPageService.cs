using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Service.Interfaces;

public interface IDashboardPageService
{
    public Task<DashboardViewModel> GetDashboardPage(string? TimePeriod, DateTime? startDate, DateTime? endDate);
}
