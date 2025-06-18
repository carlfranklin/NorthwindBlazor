using System;
using System.Collections.Generic;

namespace NorthwindBlazor.Models;

public partial class OrderDetailsExtended
{
    public int? OrderId { get; set; }

    public int? ProductId { get; set; }

    public string? ProductName { get; set; }

    public double? UnitPrice { get; set; }

    public int? Quantity { get; set; }

    public double? Discount { get; set; }

    public double? ExtendedPrice { get; set; }
}
