using PizzaShop.Entity.ViewModels;
using PizzaShop.Entity.Data;

namespace PizzaShop.Repository.Interfaces;

public interface ITableRepository
{
    List<TableViewModel> GetAllTablesAsync();
    public bool DeleteTable(int id, int userId);

    public bool AddTable(Table model);

    public bool UpdateTable(Table model);

    public Table IsTableExist(string name, int sectionId, int? tableId);
    public Task<TableViewModel> GetTableById(int id);
    public List<Table> GetTablesBySectionId(int sectionId, int pageSize, int pageIndex, string? searchString);
    public int GetTableCountBySectionId(int sId, string? searchString);
    public Task<List<Table>> GetAvailableTablesBySectionId(int id);
    public Task<bool> UpdateTableStatus(int id, bool isAvailable, int userId);

}
