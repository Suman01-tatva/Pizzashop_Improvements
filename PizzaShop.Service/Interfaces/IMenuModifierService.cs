using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Service.Interfaces;

public interface IMenuModifierService
{
    Task<List<MenuModifierGroupViewModel>> GetAllMenuModifierGroupAsync();
    public Task<List<MenuModifierViewModel>> GetModifiersByModifierGroup(int id, int pageSize, int pageIndex, string? searchString);
    public Task<ModifierTabViewModel> GetModifierTabDetails(int ModifierGroupId, int pageSize, int pageIndex, string? searchString);
    public int GetModifiersCountByCId(int mId, string? searchString);
    public bool AddModifier(AddEditModifierViewModel model, int role);
    public bool EditModifier(AddEditModifierViewModel model, int userId);
    public void DeleteModifier(int id);
    public bool DeleteMultipleModifiers(int[] modifierIds);
    public AddEditModifierViewModel GetModifierByid(int id);
    public Task<List<MenuModifierViewModel>> GetModifiersByGroupId(int id);
    public Task<AddEditExistingModifiersViewModel> GetAllModifiers(int pageSize, int pageIndex, string? searchString);
    public bool AddModifierGroup(MenuModifierGroupViewModel model, int userId);
    public bool EditModifierGroup(MenuModifierGroupViewModel model, int userId);
    public MenuModifierGroupViewModel GetEditModifierGroupDetail(int id);
    public bool IsModifierGrpExist(string name, int? id);
    public Task<bool> DeleteModifierGroup(int id);

}
