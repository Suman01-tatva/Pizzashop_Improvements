using System.Threading.Tasks;
using System.Transactions;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Interfaces;
using PizzaShop.Service.Interfaces;

namespace PizzaShop.Service.Implementations;

public class MenuModifierService : IMenuModifierService
{
    private readonly IMenuModifierGroupRepository _menuModifierGroupRepository;
    private readonly IMenuModifierRepository _menuModifierRepository;
    private readonly IUnitRepository _unitRepository;

    public MenuModifierService(IMenuModifierGroupRepository menuModifierGroupRepository, IMenuModifierRepository menuModifierRepository, IUnitRepository unitRepository)
    {
        _menuModifierGroupRepository = menuModifierGroupRepository ?? throw new ArgumentNullException(nameof(menuModifierGroupRepository));
        _menuModifierRepository = menuModifierRepository ?? throw new ArgumentNullException(nameof(menuModifierRepository));
        _unitRepository = unitRepository ?? throw new ArgumentNullException(nameof(unitRepository));
    }

    public async Task<List<MenuModifierGroupViewModel>> GetAllMenuModifierGroupAsync()
    {
        var modifierGroups = _menuModifierGroupRepository.GetAllMenuModifierGroupsAsync();
        return modifierGroups;
    }

    public async Task<List<MenuModifierViewModel>> GetModifiersByGroupId(int id)
    {
        var modifiers = await _menuModifierRepository.GetModifiersByGroupId(id);
        var modifierList = modifiers.Select(i => new MenuModifierViewModel
        {
            Name = i.Name,
            Rate = i.Rate,
        }).ToList();
        return modifierList;
    }

    public async Task<List<MenuModifierViewModel>> GetModifiersByModifierGroup(int id, int pageSize, int pageIndex, string? searchString)
    {
        var modifierList = await _menuModifierRepository.GetModifiersByModifierGroupAsync(id, pageSize, pageIndex, searchString);
        var filteredModifiers = modifierList
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList().ToList();
            
        return filteredModifiers;
    }

    public async Task<ModifierTabViewModel> GetModifierTabDetails(int ModifierGroupId, int pageSize, int pageIndex, string? searchString)
    {
        var modifierGroups = _menuModifierGroupRepository.GetAllMenuModifierGroupsAsync();

        var modifierList = await _menuModifierRepository.GetModifiersByModifierGroupAsync(ModifierGroupId, pageSize, pageIndex, searchString);
        var filteredModifiers = modifierList
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList().ToList();

        var totalModifiers = _menuModifierRepository.GetModifierCountByMId(ModifierGroupId, searchString!);

        var modifierTabViewModel = new ModifierTabViewModel
        {
            modifierGroup = modifierGroups,
            modifier = filteredModifiers,
            PageSize = pageSize,
            PageIndex = pageIndex,
            TotalPage = (int)Math.Ceiling(totalModifiers / (double)pageSize),
            SearchString = searchString,
            TotalItems = totalModifiers
        };
        return modifierTabViewModel;
    }

    public int GetModifiersCountByCId(int mId, string? searchString)
    {
        return _menuModifierRepository.GetModifierCountByMId(mId, searchString!);
    }

    public bool AddModifier(AddEditModifierViewModel model, int userId)
    {
        int isExist = _menuModifierRepository.isModifierExist(model.Name.Trim(), model.Modifiergroupid);
        if (isExist > 0)
            return false;
        _menuModifierRepository.AddModifier(model, userId);
        return true;
    }

    public bool EditModifier(AddEditModifierViewModel model, int userId)
    {
        bool isExist = _menuModifierRepository.isEditModifierExist(model.Name.Trim(), model.Id, model.Modifiergroupid);
        if (isExist)
            return false;
        _menuModifierRepository.EditModifier(model, userId);
        return true;
    }

    public void DeleteModifier(int id)
    {
        _menuModifierRepository.DeleteModifier(id);
    }

    public bool DeleteMultipleModifiers(int[] modifierIds)
    {
        if (modifierIds.Length > 0)
        {
            var isDeleted = _menuModifierRepository.MultipleDeleteModifiers(modifierIds);
            if (isDeleted)
                return true;
        }
        return false;
    }

    public AddEditModifierViewModel GetModifierByid(int id)
    {
        var modifier = _menuModifierRepository.GetModifierById(id);
        var modifierGroups = _menuModifierGroupRepository.GetAllMenuModifierGroupsAsync();
        var units = _unitRepository.GetAllUnits();

        var editModifier = new AddEditModifierViewModel
        {
            Id = modifier!.Id,
            Name = modifier.Name,
            Rate = modifier.Rate,
            Quantity = modifier.Quantity,
            Description = modifier.Description,
            Modifiergroupid = (int)modifier.ModifierGroupId,
            UnitId = modifier.UnitId,
            ModifierGroups = modifierGroups,
            Units = units
        };
        return editModifier;
    }

    public async Task<AddEditExistingModifiersViewModel> GetAllModifiers(int pageSize = 5, int pageIndex = 1, string? searchString = "")
    {
        var modifiers = await _menuModifierRepository.GetAllModifiers(searchString);
        var filteredModifier = modifiers.Skip((pageIndex - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToList();
        var filteredModifiers = filteredModifier.Select(c => new MenuModifierViewModel
        {
            Id = c.Id,
            // UnitName = c.Unit.ShortName,
            ModifierGroupId = c.ModifierGroupId,
            Name = c.Name,
            Description = c.Description,
            Rate = c.Rate,
            Quantity = c.Quantity,
        }).ToList();
        var addEditModifierViewModel = new AddEditExistingModifiersViewModel
        {
            modifier = filteredModifiers,
            PageSize = pageSize,
            PageIndex = pageIndex,
            TotalPage = (int)Math.Ceiling(modifiers.Count() / (double)pageSize),
            SearchString = searchString,
            TotalItems = modifiers.Count()
        };
        return addEditModifierViewModel;
    }



    public bool AddModifierGroup(MenuModifierGroupViewModel model, int userId)
    {
        using TransactionScope transaction = new(TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            var newModifierGroup = _menuModifierGroupRepository.AddModifierGroup(model.Name, model.Description!, userId);
            if (newModifierGroup == null)
                return false;

            _menuModifierRepository.AddExistingModifiers(model.ExistingModifiers!, newModifierGroup.Id, userId);
            transaction.Complete();
            return true;
        }
        catch (System.Exception e)
        {
            transaction.Dispose();
            return false;
        }
    }

    public bool EditModifierGroup(MenuModifierGroupViewModel model, int userId)
    {
        using TransactionScope transaction = new(TransactionScopeAsyncFlowOption.Enabled);
        var oldModifiers = _menuModifierRepository.GetModifiersByModifierGroup(model.Id);

        var oldModifiersId = new List<int>();
        var modifiersId = new List<int>();
        foreach (var om in oldModifiers)
            oldModifiersId.Add(om.Id);
        foreach (var m in model.ExistingModifiers!)
            modifiersId.Add(m.Id);

        try
        {
            var modifiers = model.ExistingModifiers;

            foreach (var m in modifiers)
            {
                if (oldModifiersId.IndexOf(m.Id) == -1)
                    _menuModifierRepository.AddExistingModifier(m, model.Id, userId);
            }

            foreach (var m in oldModifiers)
            {
                if (modifiersId.IndexOf(m.Id) == -1)
                    _menuModifierRepository.DeleteModifier(m.Id);
            }

            _menuModifierGroupRepository.EditModifierGroup(model.Id, model.Name, model.Description!, userId);
            transaction.Complete();
            return true;
        }
        catch (System.Exception e)
        {
            transaction.Dispose();
            return false;
        }
    }

    public MenuModifierGroupViewModel GetEditModifierGroupDetail(int id)
    {
        var modifiergroup = _menuModifierGroupRepository.GetModifierGroupById(id);
        var modifiers = _menuModifierRepository.GetModifiersByModifierGroup(id);
        var EditModifierDetail = new MenuModifierGroupViewModel();

        EditModifierDetail.Id = modifiergroup!.Id;
        EditModifierDetail.Name = modifiergroup.Name;
        EditModifierDetail.Description = modifiergroup.Description;

        var ExistingModifiers = new List<MenuModifierViewModel>();
        if (modifiers.Count > 0)
        {
            foreach (var m in modifiers)
            {
                ExistingModifiers.Add(new MenuModifierViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                });
            }
        }
        EditModifierDetail.Modifiers = ExistingModifiers;
        return EditModifierDetail;
    }

    public bool IsModifierGrpExist(string name, int? id)
    {
        var ModifierGrp = _menuModifierGroupRepository.IsModifierGrpExist(name);
        if (ModifierGrp != null && ModifierGrp.Id != id)
            return true;

        return false;
    }

    public async Task<bool> DeleteModifierGroup(int id)
    {
        try
        {
            bool isDeleted = _menuModifierGroupRepository.DeleteModifierGroup(id);
            if (isDeleted)
            {
                var modifiers = await _menuModifierRepository.GetModifiersByGroupId(id);
                foreach (var modifier in modifiers)
                {
                    _menuModifierRepository.DeleteModifier(modifier.Id);
                }
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

}
