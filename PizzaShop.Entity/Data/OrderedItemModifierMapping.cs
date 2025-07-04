﻿using System;
using System.Collections.Generic;

namespace PizzaShop.Entity.Data;

public partial class OrderedItemModifierMapping
{
    public int Id { get; set; }

    public int OrderItemId { get; set; }

    public int ModifierId { get; set; }

    public int? QuantityOfModifier { get; set; }

    public decimal? RateOfModifier { get; set; }

    public decimal? TotalAmount { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public int? ModifiedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? ModifiedByNavigation { get; set; }

    public virtual Modifier Modifier { get; set; } = null!;

    public virtual OrderedItem OrderItem { get; set; } = null!;
}
