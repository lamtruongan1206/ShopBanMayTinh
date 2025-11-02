using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SellComputer.Models.Entities;

public partial class Role
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    [JsonIgnore]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
