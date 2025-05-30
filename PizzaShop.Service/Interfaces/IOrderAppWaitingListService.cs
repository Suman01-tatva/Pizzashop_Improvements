using System.Text.Json.Nodes;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Service.Interfaces;

public interface IOrderAppWaitingListService
{
    public Task<AddEditWatingTokenViewModel> GetAddEditWaitingToken(int? id);
    public Task<WaitingTokenListPageViewModel> GetWaitingListPage(int? id);
    public Task<bool> AddEditWaitingToken(AddEditWatingTokenViewModel model, int userId);
    public Task<List<SectionViewModel>> GetSectionList();
    public Task<bool> DeleteWaitingtoken(int id, int userId);
    public Task<List<CustomerSuggestionListViewModel>> SearchCustomerByEmail(string? email);
    public Task<JsonArray> GetTablesBySectionId(int id);
    public Task<AssignTablePageViewModel> GetWaitingTokensById(int id);
}
