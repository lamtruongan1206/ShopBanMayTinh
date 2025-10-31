using System;
using System.Collections.Generic;

namespace SellComputer.Models.Entities;

public partial class BillDetail
{
    public Guid BillId { get; set; }

    public Guid ProductId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public virtual Computer Product { get; set; } = null!;
}
