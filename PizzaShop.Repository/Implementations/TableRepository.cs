using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PizzaShop.Entity.Data;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Interfaces;

namespace PizzaShop.Repository.Implementations;

public class TableRepository : ITableRepository
{
    private readonly PizzashopContext _context;

    public TableRepository(PizzashopContext context)
    {
        _context = context;
    }

    public List<TableViewModel> GetAllTablesAsync()
    {
        var tables = _context.Tables.Where(t => t.IsDeleted == false)
        .Select(t => new TableViewModel
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
        })
        .OrderBy(t => t.Name).ToList();
        return tables;
    }

    public int GetTableCountBySectionId(int sId, string? searchString)
    {
        var tableQuery = _context.Tables.Where(i => i.SectionId == sId && i.IsDeleted == false);
        if (!string.IsNullOrEmpty(searchString))
        {
            searchString = searchString.Trim().ToLower();

            tableQuery = tableQuery.Where(n =>
                n.Name!.ToLower().Contains(searchString)
            );
        }
        int count = tableQuery.ToList()!.Count();
        return count;
    }

    public List<Table> GetTablesBySectionId(int sectionId, int pageSize, int pageIndex, string? searchString)
    {
        var tables = _context.Tables.Where(c => c.SectionId == sectionId && c.IsDeleted == false);
        searchString = searchString?.Trim().ToLower();
        if (!string.IsNullOrEmpty(searchString))
        {
            tables = tables.Where(i => i.Name.ToLower().Contains(searchString.ToLower()));
        }

        var tableList = tables.OrderBy(u => u.Name)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return tableList;
    }

    public bool DeleteTable(int id, int userId)
    {
        var table = _context.Tables.FirstOrDefault(i => i.Id == id);
        if (table!.IsAvailable == false)
        {
            return false;
        }
        table!.IsDeleted = true;
        table.ModifiedBy = userId;
        _context.SaveChanges();

        return true;
    }

    public bool AddTable(Table model)
    {
        try
        {
            _context.Tables.Add(model);
            _context.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception Message: " + ex.Message);
            if (ex.InnerException != null)
            {
                Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
            }
            return false;
        }
    }

    public bool UpdateTable(Table model)
    {
        try
        {
            _context.Tables.Update(model);
            _context.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception Message: " + ex.Message);
            if (ex.InnerException != null)
            {
                Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
            }
            return false;
        }
    }

    public async Task<TableViewModel> GetTableById(int id)
    {
        var table = await _context.Tables
            .Where(t => t.Id == id && t.IsDeleted == false)
            .Select(t => new TableViewModel
            {
                Id = t.Id,
                Name = t.Name,
                SectionId = t.SectionId,
                Capacity = t.Capacity,
                IsAvailable = t.IsAvailable
            })
            .FirstOrDefaultAsync();

        return table!;
    }

    public Table IsTableExist(string name, int sectionId, int? tableId = null)
    {
        name = name.Trim().ToLower();
        var table = _context.Tables.FirstOrDefault(t => t.Name.ToLower() == name && t.SectionId == sectionId && (t.IsDeleted == false || t.IsDeleted == null));

        if (tableId.HasValue)
        {
            var existingTable = _context.Tables.FirstOrDefault(t => t.Id == tableId && (t.IsDeleted == false || t.IsDeleted == null));
            if (table != null && table.Id != tableId)
            {
                return table;
            }
            return existingTable;
        }
        return table;

    }

    public async Task<List<Table>> GetAvailableTablesBySectionId(int id)
    {
        return await _context.Tables
       .Where(t => t.IsDeleted == false
                   && t.SectionId == id
                   && t.IsAvailable == true
                   && (
                       !t.TableOrderMappings.Any() ||
                       t.TableOrderMappings.All(m => m.Order!.OrderStatus != 1)
                   ))
       .Include(t => t.TableOrderMappings)
           .ThenInclude(m => m.Order)
       .ToListAsync();
    }

    public async Task<bool> UpdateTableStatus(int id, bool isAvailable, int userId)
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync("CALL update_table_status({0},{1},{2})", id, isAvailable, userId);
            // table!.IsAvailable = isAvailable;
            // table.ModifiedBy = userId;
            // table.ModifiedAt = DateTime.UtcNow;
            // _context.Tables.Update(table);
            // await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception Message: " + ex.Message);
            if (ex.InnerException != null)
            {
                Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
            }
            return false;
        }
    }
}