namespace PizzaShop.Entity.ViewModels;

public class TaxesViewModel
{
    public int Id { get; set; }

    public string? Name { get; set; } = null!;

    public bool? Type { get; set; }

    public decimal? FlatAmount { get; set; }

    public decimal? Percentage { get; set; }

    public decimal? TaxValue { get; set; }

    public decimal? TaxAmount { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDefault { get; set; }

    public bool? IsDeleted { get; set; }

    public decimal? CalculatedTax { get; set; }

}
