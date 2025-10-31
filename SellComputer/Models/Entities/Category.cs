using System;
using System.Collections.Generic;

namespace SellComputer.Models.Entities;

public partial class Category
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Decription { get; set; }

    public DateOnly? CreateAt { get; set; }

    public virtual ICollection<Computer> Computers { get; set; } = new List<Computer>();
}
