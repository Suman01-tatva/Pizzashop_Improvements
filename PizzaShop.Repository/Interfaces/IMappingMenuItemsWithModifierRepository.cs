using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Repository.Interfaces;

public interface IMappingMenuItemsWithModifierRepository
{
    public Task<bool> AddMapping(List<ItemModifierViewModel> modifierGroupData, int itemId, int role);
    public Task<bool> EditMappings(List<ItemModifierViewModel> modifierGroupData, int itemId, int role);
    public Task<List<ItemModifierViewModel>> ModifierGroupDataByItemId(int itemId);
    public void DeleteMapping(List<int> mappingIds);
    public Task<List<MappingMenuItemsWithModifier>> GetItemModfiers(int itemId);

}
