using System;
using System.Collections.Generic;

namespace WebBanMayTinh.Models.Entities;

public partial class Computer
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Manufacturer { get; set; }

    public decimal? Price { get; set; }

    public int? Quantity { get; set; }

    public DateOnly? UpdateAt { get; set; }

    public DateOnly? CreateAt { get; set; }

    public Guid? CategoriesId { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Category? Categories { get; set; }

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<Specification> Specifications { get; set; } = new List<Specification>();
  
}
