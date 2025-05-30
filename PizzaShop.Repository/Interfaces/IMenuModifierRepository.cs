using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Repository.Interfaces;

public interface IMenuModifierRepository
{
    public Task<List<MenuModifierViewModel>> GetModifiersByModifierGroupAsync(int id, int pageSize, int pageIndex, string? searchString);
    public int GetModifierCountByMId(int mId, string? searchString);
    public void DeleteModifier(int id);
    public bool MultipleDeleteModifiers(int[] modifiersIds);
    public void AddModifier(AddEditModifierViewModel model, int userId);
    public int isModifierExist(string name, int? modGrpId);
    public bool isEditModifierExist(string name, int? id, int? modGrpId);
    public void EditModifier(AddEditModifierViewModel model, int userId);
    public Modifier? GetModifierById(int id);
    public Task<List<Modifier>> GetModifiersByGroupId(int id);
    public Task<List<Modifier>> GetAllModifiers(string? searchString);
    public void AddExistingModifiers(List<ExistingModifierViewModel> modifiers, int modifierGroupId, int userId);
    public List<Modifier>? GetModifiersByModifierGroup(int? id);
    public void AddExistingModifier(ExistingModifierViewModel modifier, int modifierGroupId, int userId);
}
