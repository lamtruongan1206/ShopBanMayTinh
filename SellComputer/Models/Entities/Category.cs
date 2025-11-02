using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SellComputer.Models.Entities;

public partial class Category
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public DateOnly? CreateAt { get; set; }
    [JsonIgnore]

    public virtual ICollection<Computer> Computers { get; set; } = new List<Computer>();
}
