using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Repository.Interfaces;

public interface IMenuModifierGroupRepository
{
    List<MenuModifierGroupViewModel> GetAllMenuModifierGroupsAsync();
    public ModifierGroup? AddModifierGroup(string name, string description, int userId);
    public void EditModifierGroup(int? id, string name, string description, int userId);
    public ModifierGroup? GetModifierGroupById(int id);
    public ModifierGroup IsModifierGrpExist(string name);
    public bool DeleteModifierGroup(int id);

}
