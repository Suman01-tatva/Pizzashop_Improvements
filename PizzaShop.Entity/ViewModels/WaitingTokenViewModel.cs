namespace PizzaShop.Entity.ViewModels;

public class WaitingTokenViewModel
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerEmail { get; set; }

    public string? MobileNumber { get; set; }

    public int SectionId { get; set; }

    public int? TableId { get; set; }

    public int? NoOfPeople { get; set; }

    public bool? IsDeleted { get; set; }

    public bool? IsAssigned { get; set; }

    public string? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }
    public string? WaitingTime { get; set; }

}
