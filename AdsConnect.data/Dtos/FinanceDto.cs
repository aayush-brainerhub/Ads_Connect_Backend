using System.ComponentModel.DataAnnotations;

namespace AdsConnect.data.Dtos
{
    public class FinancialOverviewDto
    {
        public decimal grossGMV { get; set; }
        public decimal platformRevenue { get; set; }
        public decimal netProviderPayouts { get; set; }
        public decimal pendingInvoicesAmount { get; set; }
        public decimal completedPaymentsAmount { get; set; }
        public int activeCommissionRules { get; set; }
    }

    public class CommissionRuleDto
    {
        public Guid id { get; set; }
        public string scope { get; set; } = string.Empty;
        public Guid? channelId { get; set; }
        public string? channelName { get; set; }
        public Guid? providerTypeId { get; set; }
        public string? providerTypeName { get; set; }
        public decimal percentageRate { get; set; }
        public decimal fixedAmount { get; set; }
        public decimal? minFee { get; set; }
        public decimal? maxFee { get; set; }
        public string currency { get; set; } = "USD";
        public short priority { get; set; }
        public bool isActive { get; set; }
        public string effectiveFrom { get; set; } = string.Empty;
        public string? effectiveTo { get; set; }
    }

    public class CreateCommissionRuleDto
    {
        [Required]
        public string scope { get; set; } = "Global";
        public Guid? channelId { get; set; }
        public string? channelName { get; set; }
        public Guid? providerTypeId { get; set; }
        public string? providerTypeName { get; set; }
        public decimal percentageRate { get; set; }
        public decimal fixedAmount { get; set; }
        public decimal? minFee { get; set; }
        public decimal? maxFee { get; set; }
        public string currency { get; set; } = "USD";
        public short priority { get; set; } = 10;
    }

    public class InvoiceLineDto
    {
        public Guid id { get; set; }
        public Guid invoiceId { get; set; }
        public string description { get; set; } = string.Empty;
        public int quantity { get; set; }
        public decimal unitPrice { get; set; }
        public decimal lineTotal { get; set; }
        public string itemType { get; set; } = string.Empty;
    }

    public class InvoiceDto
    {
        public Guid id { get; set; }
        public string invoiceNumber { get; set; } = string.Empty;
        public Guid bookingId { get; set; }
        public string bookingNumber { get; set; } = string.Empty;
        public string direction { get; set; } = string.Empty;
        public Guid advertiserId { get; set; }
        public string advertiserName { get; set; } = string.Empty;
        public Guid providerId { get; set; }
        public string providerName { get; set; } = string.Empty;
        public decimal subtotal { get; set; }
        public decimal taxAmount { get; set; }
        public decimal totalAmount { get; set; }
        public decimal amountPaid { get; set; }
        public string currency { get; set; } = "USD";
        public string status { get; set; } = string.Empty;
        public string issuedDate { get; set; } = string.Empty;
        public string dueDate { get; set; } = string.Empty;
        public string? paidDate { get; set; }
        public string? documentUrl { get; set; }
        public List<InvoiceLineDto> lines { get; set; } = new();
    }

    public class PaymentDto
    {
        public Guid id { get; set; }
        public string paymentNumber { get; set; } = string.Empty;
        public Guid invoiceId { get; set; }
        public string invoiceNumber { get; set; } = string.Empty;
        public Guid bookingId { get; set; }
        public string bookingNumber { get; set; } = string.Empty;
        public decimal amount { get; set; }
        public string currency { get; set; } = "USD";
        public string paymentMethod { get; set; } = string.Empty;
        public string transactionReference { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public string paymentDate { get; set; } = string.Empty;
        public string? notes { get; set; }
    }

    public class PayInvoiceDto
    {
        [Required]
        public Guid invoiceId { get; set; }

        [Required]
        public string paymentMethod { get; set; } = "Stripe";

        public string? notes { get; set; }
    }
}
