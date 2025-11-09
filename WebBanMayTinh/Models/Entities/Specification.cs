using System;
using System.Collections.Generic;

namespace WebBanMayTinh.Models.Entities;

public partial class Specification
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public int? Value { get; set; }

    public string? Description { get; set; }

    public Guid? ProductId { get; set; }

    public virtual Computer? Product { get; set; }
}
