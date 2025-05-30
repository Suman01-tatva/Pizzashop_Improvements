using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Service.Interfaces;

public interface ITableService
{
    public List<TableViewModel> GetAllTables();
    public bool DeleteTable(int id, int userId);
    public bool MultiDeleteTable(int[] tableIds, int userId);
    public bool AdddTable(TableViewModel model, int userId);
    public bool UpdateTable(TableViewModel model, int userId);
    public Task<TableViewModel> GetTableById(int id);
    public int GetTableCountBySectionId(int sId, string? searchString);
    public TableSectionViewModel GetTablesBySectionId(int sectionId, int pageSize, int pageIndex, string? searchString);

}
