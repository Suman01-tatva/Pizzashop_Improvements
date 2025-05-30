namespace PizzaShop.Entity.ViewModels;

public class KOTResultViewModel
{
    public int Id { get; set; }
    public DateTime Created_At { get; set; }
    public DateTime? Modified_At { get; set; }
    public string Notes { get; set; }
    public bool Is_Deleted { get; set; }
    public int Order_Status { get; set; }


    public int Ordered_Item_Id { get; set; }
    public int Ready_Item_Quantity { get; set; }
    public int Quantity { get; set; }
    public string Instruction { get; set; }
    public int Order_Item_Status { get; set; }


    public int? Ordered_Item_Modifier_Mapping_Id { get; set; }
    public int Menu_Item_Id { get; set; }
    public string Menu_Item_Name { get; set; }
    public int Menu_Item_Category_Id { get; set; }

    public int? Modifier_Id { get; set; }
    public string Modifier_Name { get; set; }


    public int? Table_Order_Mapping_Id { get; set; }
    public int? Table_Id { get; set; }
    public string Table_Name { get; set; }
    public int? Section_Id { get; set; }
    public string Section_Name { get; set; }
}