using System;
using System.Collections.Generic;

namespace WebBanMayTinh.Models.Entities;

public partial class Cart
{
    public Guid Id { get; set; }

    public int? Quantity { get; set; }

    public DateOnly? CreateAt { get; set; }

    public Guid? ProductId { get; set; }

    public Guid? UserId { get; set; }

    public virtual Computer? Product { get; set; }

    public virtual User? User { get; set; }
}
