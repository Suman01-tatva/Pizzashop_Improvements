namespace PizzaShop.Entity.ViewModels;

public class KotTableDetailViewModel
{
    public int? orderNo { get; set; }
    public string? OrderDuration { get; set; }
    public string? SectionName { get; set; }
    public string? TableName { get; set; }
    public string? OrderInstructions { get; set; }
    public int[]? PreparedItemList { get; set; }
    public int[]? InProgressItemList { get; set; }
    public int? CategoryId { get; set; }
    public int? status { get; set; }
    public List<ItemList>? OrderedItems { get; set; }

}

public class ItemList
{
    public int? OrderItemId { get; set; }
    public string? ItemName { get; set; }
    public string? Instruction { get; set; }
    public int? Quantity { get; set; }
    public int? ReadyItemQuantity { get; set; }
    public List<ModifierList>? ModifierList { get; set; }

}

public class ModifierList
{
    public string? Name { get; set; }

}

public class UpdateOrderItems
{
    public int? Id { get; set; }
    public int? Quantity { get; set; }

}