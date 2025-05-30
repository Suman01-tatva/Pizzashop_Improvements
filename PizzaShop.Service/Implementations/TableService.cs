using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Interfaces;
using PizzaShop.Service.Interfaces;

namespace PizzaShop.Service.Implementations;

public class TableService : ITableService
{
    private readonly ITableRepository _tableRepository;
    private readonly ISectionRepository _sectionRepository;

    public TableService(
        ITableRepository tableService, ISectionRepository sectionRepository)
    {
        _tableRepository = tableService;
        _sectionRepository = sectionRepository;
    }

    public List<TableViewModel> GetAllTables()
    {
        var sections = _tableRepository.GetAllTablesAsync();
        return sections;
    }

    public TableSectionViewModel GetTablesBySectionId(int sectionId, int pageSize, int pageIndex, string? searchString)
    {
        var tables = _tableRepository.GetTablesBySectionId(sectionId, pageSize, pageIndex, searchString);
        var filteredTables = tables.Select(t => new TableViewModel
        {
            Id = t.Id,
            SectionId = t.SectionId,
            Name = t.Name,
            Capacity = t.Capacity,
            IsAvailable = t.IsAvailable,
            IsDeleted = t.IsDeleted,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = t.CreatedBy,
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = t.ModifiedBy
        }).ToList();
        var totalTable = _tableRepository.GetTableCountBySectionId(sectionId, searchString);
        var model = new TableSectionViewModel
        {
            Tables = filteredTables,
            PageSize = pageSize,
            PageIndex = pageIndex,
            SearchString = searchString,
            TotalPage = (int)Math.Ceiling(totalTable / (double)pageSize),
            TotalItems = totalTable
        };
        return model;
    }

    public int GetTableCountBySectionId(int sId, string? searchString)
    {
        return _tableRepository.GetTableCountBySectionId(sId, searchString!);
    }

    public bool DeleteTable(int id, int userId)
    {
        var isDelete = _tableRepository.DeleteTable(id, userId);
        return isDelete;
    }

    public bool MultiDeleteTable(int[] tableIds, int userId)
    {
        try
        {
            var occupied = false;
            foreach (var table in tableIds)
            {
                var isDelete = _tableRepository.DeleteTable(table, userId);
                if (isDelete == false)
                {
                    occupied = true;
                    break;
                }
            }
            if (occupied == true)
                return false;
            return true;
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public bool AdddTable(TableViewModel model, int userId)
    {
        Table table = _tableRepository.IsTableExist(model.Name, model.SectionId, model.Id);
        if (table != null)
        {
            return false;
        }
       
        var newTable = new Table
        {
            Name = model.Name.Trim(),
            SectionId = model.SectionId,
            Capacity = model.Capacity,
            IsAvailable = true,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        return _tableRepository.AddTable(newTable);
    }

    public async Task<TableViewModel> GetTableById(int id)
    {
        return await _tableRepository.GetTableById(id);
    }

    public bool UpdateTable(TableViewModel model, int userId)
    {
        Table table = _tableRepository.IsTableExist(model.Name, model.SectionId, model.Id);
        if (table != null && table.Id != model.Id)
        {
            return false;
        }

        table!.Name = model.Name.Trim();
        table.SectionId = model.SectionId;
        table.Capacity = model.Capacity;
        table.IsAvailable = model.IsAvailable == null ? true : model.IsAvailable;
        table.ModifiedBy = userId;
        table.ModifiedAt = DateTime.UtcNow;

        return _tableRepository.UpdateTable(table);
    }

}
