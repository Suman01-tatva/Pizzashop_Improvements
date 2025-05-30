namespace PizzaShop.Web.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public int statusCode { get; set; }
    public string? errorText { get; set; }
    public string? subText { get; set; }
}
