namespace PizzaShop.Entity.ViewModels;

public class AssignTableViewModel
{
    public int? CustomerId { get; set; }

    public int? waitingTokenId { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerEmail { get; set; }

    public string? MobileNumber { get; set; }

    public int? NoOfPerson { get; set; }
    public int? SectionId { get; set; }

    public List<tableDetails>? TableOrders { get; set; }

}

public class tableDetails
{
    public int? id { get; set; }

    public int? capacity { get; set; }
}