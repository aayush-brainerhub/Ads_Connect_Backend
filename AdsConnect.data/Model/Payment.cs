using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class Payment
{
    public Guid PaymentId { get; set; }

    public Guid BookingId { get; set; }

    public Guid? InvoiceId { get; set; }

    public string Direction { get; set; } = null!;

    public Guid? PayerUserId { get; set; }

    public Guid? PayeeUserId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public string? PaymentMethod { get; set; }

    public string? Gateway { get; set; }

    public string? TransactionReference { get; set; }

    public string GatewayPayload { get; set; } = null!;

    public string? IdempotencyKey { get; set; }

    public string Status { get; set; } = null!;

    public string? FailureReason { get; set; }

    public DateTime? PaymentDate { get; set; }

    public DateTime? SettledDate { get; set; }

    public Guid? ParentPaymentId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual ICollection<Payment> InverseParentPayment { get; set; } = new List<Payment>();

    public virtual Invoice? Invoice { get; set; }

    public virtual Payment? ParentPayment { get; set; }

    public virtual AppUser? PayeeUser { get; set; }

    public virtual AppUser? PayerUser { get; set; }
}
