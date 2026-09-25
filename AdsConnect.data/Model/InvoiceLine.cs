using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class InvoiceLine
{
    public Guid InvoiceLineId { get; set; }

    public Guid InvoiceId { get; set; }

    public string Description { get; set; } = null!;

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? LineTotal { get; set; }

    public decimal TaxRate { get; set; }

    public short SortOrder { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;
}
