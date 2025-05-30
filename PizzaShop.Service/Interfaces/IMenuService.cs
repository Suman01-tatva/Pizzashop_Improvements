using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Service.Interfaces;

public interface IMenuService
{
    Task<List<MenuCategoryViewModel>> GetAllMenuCategoriesAsync();

    Task<List<MenuItemViewModel>> GetItemsByCategory(int categoryId, int pageSize, int pageIndex, string? searchString);

    Task<bool> AddNewCategory(string category, AddEditCategoryViewModel model);

    public Task<AddEditCategoryViewModel> GetCategoryDetailById(int id);
    public Task<bool> EditCategory(AddEditCategoryViewModel model, int categoryId);

    public Task<ItemTabViewModel> GetItemTabDetails(int categoryId, int pageSize, int pageIndex, string? searchString);

    public bool SoftDeleteCategory(int id);

    public int GetItemsCountByCId(int cId, string? searchString);

    public bool FindCategoryByName(string name);

    public List<Unit> GetAllUnits();

    public Task<bool> AddNewItem(MenuItemViewModel item, int userId);

    public bool IsItemExist(string name, int catId, int? itemId);

    public Task<MenuItemViewModel> GetMenuItemById(int id);

    public Task<List<MenuModifierViewModel>?> GetModifiersByModifierGroup(int? id);

    public void DeleteMenuItem(int id);

    public bool MultiDeleteMenuItem(int[] itemIds);

    public Task EditItem(MenuItemViewModel model, int userId);

    public Task<List<MenuItemViewModel>> GetAllMenuItems();

    public Task<List<MenuItemViewModel>> GetMenuItemsByCategoryId(int categoryId, string searchString, string type);

}
