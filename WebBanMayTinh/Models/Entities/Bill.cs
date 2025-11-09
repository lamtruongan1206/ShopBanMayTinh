using System;
using System.Collections.Generic;

namespace WebBanMayTinh.Models.Entities;

public partial class Bill
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public DateOnly? CreateAt { get; set; }

    public string? PaymentMethod { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? Status { get; set; }

    public virtual User? User { get; set; }
}
