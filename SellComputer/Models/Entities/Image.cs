using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SellComputer.Models.Entities;

public partial class Image
{
    public Guid Id { get; set; }

    public string? Url { get; set; }

    public bool IsMain { get; set; }

    public int? OrderNumber { get; set; }

    public Guid? ProductId { get; set; }

    [JsonIgnore]
    public virtual Computer? Product { get; set; }
}
