namespace PizzaShop.Entity.ViewModels;

public class TableSectionViewModel
{
    public List<TableViewModel> Tables { get; set; } = new List<TableViewModel>();
    public List<SectionViewModel> Sections { get; set; } = new List<SectionViewModel>();
    public int PageSize { get; set; }
    public int PageIndex { get; set; }
    public string? SearchString { get; set; } = null!;
    public int TotalPage { get; set; }
    public int TotalItems { get; set; }
}
