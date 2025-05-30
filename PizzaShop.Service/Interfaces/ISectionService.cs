using System.Security.Claims;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Service.Interfaces;

public interface ISectionService
{
    public List<SectionViewModel> GetAllSections();
    Task<SectionViewModel?> GetSectionByIdAsync(int id);
    Task<bool> AddSectionAsync(SectionViewModel model, int userId);
    Task<bool> UpdateSectionAsync(SectionViewModel model, int userId);
    Task<string> DeleteSectionAsync(int id, bool softDelete, int userId);
}
