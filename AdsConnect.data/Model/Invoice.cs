using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class Invoice
{
    public Guid InvoiceId { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public Guid BookingId { get; set; }

    public string Direction { get; set; } = null!;

    public Guid AdvertiserId { get; set; }

    public Guid ProviderId { get; set; }

    public decimal Subtotal { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal AmountPaid { get; set; }

    public string Currency { get; set; } = null!;

    public string BillingSnapshot { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? IssuedDate { get; set; }

    public DateOnly? DueDate { get; set; }

    public DateTime? PaidDate { get; set; }

    public string? DocumentUrl { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Advertiser Advertiser { get; set; } = null!;

    public virtual Booking Booking { get; set; } = null!;

    public virtual ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Provider Provider { get; set; } = null!;
}
