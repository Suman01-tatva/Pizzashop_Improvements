using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class MenuItemRepository : IMenuItemsRepository
{
    private readonly PizzashopContext _context;

    public MenuItemRepository(PizzashopContext context)
    {
        _context = context;
    }

    public async Task<List<MenuItem>> GetItemsByCategory(int categoryId, int pageSize, int pageIndex, string? searchString)
    {
        var itmes = _context.MenuItems.Where(c => c.CategoryId == categoryId && c.IsDeleted == false);
        searchString = searchString?.Trim().ToLower();
        if (!string.IsNullOrEmpty(searchString))
        {
            itmes = itmes.Where(i => i.Name.ToLower().Contains(searchString.ToLower()));
        }

        var itemList = itmes.OrderBy(u => u.Name)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return itemList;
    }

    public async Task<List<MenuItem>> GetItemsByCategoryId(int categoryId, string searchString, string type)
    {
        var items = _context.MenuItems.Where(c => c.IsDeleted == false)
                                      .Include(i => i.Unit).AsQueryable();
        if (categoryId == -1)
            items = items.Where(i => i.IsFavourite == true);
        else if (categoryId != 0)
            items = items.Where(i => i.CategoryId == categoryId);

        searchString = searchString?.Trim().ToLower()!;
        if (!string.IsNullOrEmpty(searchString))
        {
            items = items.Where(i => i.Name.ToLower().Contains(searchString.ToLower()));
        }

        if (type == "veg")
        {
            items = items.Where(i => i.Type == true);
        }
        else if (type == "nonveg")
        {
            items = items.Where(i => i.Type == false);
        }

        var itemList = await items.OrderBy(i => i.Name).ToListAsync();
        return itemList;
    }

    public async Task<List<MenuItem>> GetAllMenuItems()
    {
        var items = await _context.MenuItems.Where(i => i.IsDeleted == false)
                                  .Include(i => i.Unit)
                                  .OrderBy(i => i.Name)
                                  .ToListAsync();
        return items;
    }

    public int GetItemsCountByCId(int cId, string? searchString)
    {
        var itemQuery = _context.MenuItems.Where(i => i.CategoryId == cId && i.IsDeleted == false);
        if (!string.IsNullOrEmpty(searchString))
        {
            searchString = searchString.Trim().ToLower();

            itemQuery = itemQuery.Where(n =>
                n.Name!.ToLower().Contains(searchString)
            );
        }
        int count = itemQuery.ToList()!.Count();
        return count;
    }

    public bool AddNewItem(MenuItem model)
    {
        try
        {
            _context.MenuItems.Add(model);
            _context.SaveChanges();
            return true;
        }
        catch (Exception Error)
        {
            Console.WriteLine(Error);
            return false;
        }
    }

    public MenuItem IsItemExist(string name, int catId)
    {
        name = name.Trim().ToLower();
        var item = _context.MenuItems.FirstOrDefault(i => i.Name.ToLower() == name && i.CategoryId == catId && i.IsDeleted == false);

        return item!;
    }

    public MenuItem GetMenuItemById(int id)
    {
        var item = _context.MenuItems.FirstOrDefault(i => i.Id == id);
        return item!;
    }


    public void EditMenuItem(MenuItemViewModel model, int userId)
    {
        var menuItem = _context.MenuItems.FirstOrDefault(i => i.Id == model.Id);

        menuItem!.CategoryId = model.CategoryId;
        menuItem.Name = model.Name;
        menuItem.Type = model.ItemType;
        menuItem.Rate = model.Rate;
        menuItem.Quantity = model.Quantity;
        menuItem.IsAvailable = model.IsAvailable;
        menuItem.UnitId = model.UnitId;
        menuItem.IsDefaultTax = model.IsDefaultTax;
        menuItem.TaxPercentage = model.TaxPercentage;
        menuItem.ShortCode = model.ShortCode!;
        menuItem.Description = model.Description!;
        if (!string.IsNullOrEmpty(model.Image))
        {
            menuItem.Image = model.Image;
        }
        menuItem.ModifiedBy = userId;
        menuItem.ModifiedAt = DateTime.UtcNow;
        _context.SaveChanges();
    }

    public void DeleteMenuItem(int id)
    {
        var menuItem = _context.MenuItems.FirstOrDefault(i => i.Id == id);
        menuItem!.IsDeleted = true;
        _context.SaveChanges();
    }


    public bool MultipleDeleteMenuItem(int[] itemIds)
    {
        try
        {
            foreach (var id in itemIds)
            {
                var menuItem = _context.MenuItems.FirstOrDefault(i => i.Id == id);
                menuItem!.IsDeleted = true;
                _context.Update(menuItem);
            }

            _context.SaveChanges();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public bool UpdateMenuItem(MenuItem item)
    {
        try
        {
            var updatedItem = _context.MenuItems.Update(item);
            _context.SaveChanges();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<bool> MarkAsFavourite(int itemId)
    {
        MenuItem item = await _context.MenuItems.FirstOrDefaultAsync(i => i.Id == itemId && i.IsDeleted == false);
        if (item != null)
        {
            item.IsFavourite = !item.IsFavourite;
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }
}
