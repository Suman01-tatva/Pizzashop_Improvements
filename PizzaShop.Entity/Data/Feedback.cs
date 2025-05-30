using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Data;

public partial class Feedback
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string? Comments { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public Decimal? AvgRating { get; set; }

    public int? Food { get; set; }

    public int? Service { get; set; }

    public int? Ambience { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual Order Order { get; set; } = null!;
}
