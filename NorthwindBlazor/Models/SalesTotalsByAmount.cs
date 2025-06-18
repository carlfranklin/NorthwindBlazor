using System;
using System.Collections.Generic;

namespace NorthwindBlazor.Models;

public partial class SalesTotalsByAmount
{
    public byte[]? SaleAmount { get; set; }

    public int? OrderId { get; set; }

    public string? CompanyName { get; set; }

    public DateTime? ShippedDate { get; set; }
}
