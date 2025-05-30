using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Repository.Interfaces;

public interface IMenuItemsRepository
{
    Task<List<MenuItem>> GetItemsByCategory(int categoryId, int pageSize, int pageIndex, string? searchString);

    public Task<List<MenuItem>> GetAllMenuItems();

    public int GetItemsCountByCId(int categoryId, string searchString);

    public bool AddNewItem(MenuItem model);

    public MenuItem IsItemExist(string name, int catId);

    public MenuItem GetMenuItemById(int id);

    public void EditMenuItem(MenuItemViewModel model, int userId);

    public void DeleteMenuItem(int id);

    public bool MultipleDeleteMenuItem(int[] itemIds);

    public bool UpdateMenuItem(MenuItem item);

    public Task<List<MenuItem>> GetItemsByCategoryId(int categoryId, string searchString, string type);

    public Task<bool> MarkAsFavourite(int itemId);

}
