using System;
using System.Collections.Generic;

namespace SellComputer.Models.Entities;

public partial class Computer
{
    public Guid id { get; set; } 

    public string? Name { get; set; }

    public string? Manufacturer { get; set; }

    public decimal? Price { get; set; }

    public int? Quantity { get; set; }

    public DateTime? UpdateAt { get; set; }

    public DateTime? CreateAt { get; set; } = DateTime.Now;

    public Guid? CategoriesId { get; set; }

    public virtual ICollection<BillDetail> BillDetails { get; set; } = new List<BillDetail>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Category? Categories { get; set; }

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<Specification> Specifications { get; set; } = new List<Specification>();
}
