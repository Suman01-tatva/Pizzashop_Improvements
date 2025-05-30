using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class MappingMenuItemsWithModifierRepository : IMappingMenuItemsWithModifierRepository
{
    private readonly PizzashopContext _context;

    public MappingMenuItemsWithModifierRepository(PizzashopContext context)
    {
        _context = context;
    }

    public async Task<bool> AddMapping(List<ItemModifierViewModel> modifierGroupData, int itemId, int id)
    {
        if (modifierGroupData.Count < 1)
            return false;

        foreach (var mgId in modifierGroupData)
        {
            var record = new MappingMenuItemsWithModifier
            {
                MenuItemId = itemId,
                ModifierGroupId = mgId.ModifierGroupId,
                MinSelectionRequired = mgId.Minselectionrequired,
                MaxSelectionAllowed = mgId.Maxselectionrequired,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = id,
                IsDeleted = false
            };

            _context.MappingMenuItemsWithModifiers.AddAsync(record);
        }
        _context.SaveChanges();
        return true;
    }

    public async Task<bool> EditMappings(List<ItemModifierViewModel> modifierGroupData, int itemId, int id)
    {
        foreach (var mgId in modifierGroupData)
        {
            var mapping = _context.MappingMenuItemsWithModifiers.FirstOrDefault(m => m.Id == mgId.Id);
            if (mapping != null)
            {
                mapping.MaxSelectionAllowed = mgId.Maxselectionrequired;
                mapping.MinSelectionRequired = mgId.Minselectionrequired;
                mapping.ModifiedBy = id;
                mapping.ModifiedAt = DateTime.UtcNow;
            }
            else
            {
                return false;
            }
        }
        _context.SaveChanges();
        return true;
    }


    public async Task<bool> EditMapping(MenuItemViewModel model, int ItemId, string role)
    {
        var oldMapping = await _context.MappingMenuItemsWithModifiers.Where(m => m.MenuItemId == ItemId).ToListAsync();
        var newMapping = model.ItemModifiersList;

        return true;
    }

    public async Task<List<ItemModifierViewModel>> ModifierGroupDataByItemId(int itemId)
    {
        var mapping = _context.MappingMenuItemsWithModifiers.Where(m => m.MenuItemId == itemId && m.IsDeleted == false).ToList();
        var ItemModifiers = new List<ItemModifierViewModel>();
        foreach (var m in mapping)
        {
            var n = _context.ModifierGroups.FirstOrDefault(mg => mg.Id == m.ModifierGroupId);
            ItemModifiers.Add(new ItemModifierViewModel
            {
                Id = m.Id,
                ModifierGroupId = m.ModifierGroupId,
                Name = n!.Name,
                Minselectionrequired = m.MinSelectionRequired,
                Maxselectionrequired = m.MaxSelectionAllowed,
            });
        }
        return ItemModifiers;
    }

    public void DeleteMapping(List<int> mappingIds)
    {
        foreach (var m in mappingIds)
        {
            var mapping = _context.MappingMenuItemsWithModifiers.FirstOrDefault(i => i.Id == m);
            mapping.IsDeleted = true;
        }
        _context.SaveChanges();
    }

    public async Task<List<MappingMenuItemsWithModifier>> GetItemModfiers(int itemId)
    {
        var itemModifiers = await _context.MappingMenuItemsWithModifiers.Where(i => i.MenuItemId == itemId)
                                                                        .Include(i => i.MenuItem)
                                                                        .Include(m => m.ModifierGroup)
                                                                        .ThenInclude(m => m.Modifiers).ToListAsync();
        return itemModifiers;
    }
}
