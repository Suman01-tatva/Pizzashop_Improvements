using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class MenuModifierGroupRepository : IMenuModifierGroupRepository
{
    private readonly PizzashopContext _context;

    public MenuModifierGroupRepository(PizzashopContext context)
    {
        _context = context;
    }
    public List<MenuModifierGroupViewModel> GetAllMenuModifierGroupsAsync()
    {
        var modifierGroups = _context.ModifierGroups
        .Where(c => c.IsDeleted == false)
        .Select(c => new MenuModifierGroupViewModel
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        }).OrderBy(m => m.Name).ToList();

        return modifierGroups;
    }

    public ModifierGroup? AddModifierGroup(string name, string description, int userId)
    {
        var neweModifierGroup = new ModifierGroup
        {
            Name = name,
            Description = description,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.ModifierGroups.Add(neweModifierGroup);
        _context.SaveChanges();
        return neweModifierGroup;
    }

    public void EditModifierGroup(int? id, string name, string description, int userId)
    {
        var modifierGroup = _context.ModifierGroups.FirstOrDefault(m => m.Id == id);
        modifierGroup!.Name = name.Trim();
        modifierGroup.Description = description.Trim();
        modifierGroup.ModifiedAt = DateTime.UtcNow;
        modifierGroup.ModifiedBy = userId;
        _context.SaveChanges();
    }

    public ModifierGroup? GetModifierGroupById(int id)
    {
        var modifierGroup = _context.ModifierGroups.FirstOrDefault(m => m.Id == id);
        return modifierGroup;
    }

    public ModifierGroup IsModifierGrpExist(string name)
    {
        var modifierGroup = _context.ModifierGroups.Where(m => m.Name.ToLower() == name.ToLower().Trim() && m.IsDeleted == false).FirstOrDefault();
        return modifierGroup!;
    }

    public bool DeleteModifierGroup(int id)
    {
        var modifierGroup = _context.ModifierGroups.FirstOrDefault(m => m.Id == id);
        if (modifierGroup != null)
        {
            modifierGroup.IsDeleted = true;
            _context.SaveChanges();
            return true;
        }
        else
            return false;
    }
}
