using System.Text.Json;
using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using AdsConnect.data.Model;
using Microsoft.EntityFrameworkCore;

namespace AdsConnect.data.Repository
{
    public class FinanceService : IFinanceService
    {
        private readonly AdsConnectContext _context;

        public FinanceService(AdsConnectContext context)
        {
            _context = context;
        }

        public async Task<FinancialOverviewDto> GetFinancialOverviewService()
        {
            var grossGMV = await _context.Bookings
                .Where(b => b.Status != "Cancelled")
                .SumAsync(b => (decimal?)b.TotalAmount) ?? 0m;

            var platformRevenue = await _context.PlatformFees
                .SumAsync(f => (decimal?)f.CalculatedAmount) ?? 0m;

            var netPayouts = await _context.Bookings
                .Where(b => b.Status == "Completed")
                .SumAsync(b => (decimal?)b.ProviderPayout) ?? 0m;

            var pendingInvoices = await _context.Invoices
                .Where(i => i.Status == "Issued" || i.Status == "Overdue")
                .SumAsync(i => (decimal?)i.TotalAmount) ?? 0m;

            var completedPayments = await _context.Payments
                .Where(p => p.Status == "Completed")
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            var activeRules = await _context.CommissionRules
                .CountAsync(r => r.IsActive);

            return new FinancialOverviewDto
            {
                grossGMV = grossGMV,
                platformRevenue = platformRevenue,
                netProviderPayouts = netPayouts,
                pendingInvoicesAmount = pendingInvoices,
                completedPaymentsAmount = completedPayments,
                activeCommissionRules = activeRules,
            };
        }

        public async Task<List<InvoiceDto>> GetInvoicesService(Guid userId, string role)
        {
            var query = _context.Invoices
                .Include(i => i.Booking)
                .Include(i => i.Advertiser)
                .Include(i => i.Provider)
                .Include(i => i.InvoiceLines)
                .AsQueryable();

            if (role == "Advertiser")
            {
                var adv = await _context.Advertisers.FirstOrDefaultAsync(a => a.UserId == userId);
                if (adv == null) return new List<InvoiceDto>();
                query = query.Where(i => i.AdvertiserId == adv.AdvertiserId);
            }
            else if (role == "Provider")
            {
                var prov = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);
                if (prov == null) return new List<InvoiceDto>();
                query = query.Where(i => i.ProviderId == prov.ProviderId);
            }

            var invoices = await query
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();

            return invoices.Select(i => new InvoiceDto
            {
                id = i.InvoiceId,
                invoiceNumber = i.InvoiceNumber,
                bookingId = i.BookingId,
                bookingNumber = i.Booking?.BookingNumber ?? string.Empty,
                direction = i.Direction,
                advertiserId = i.AdvertiserId,
                advertiserName = i.Advertiser?.BusinessName ?? "Advertiser",
                providerId = i.ProviderId,
                providerName = i.Provider?.ProviderName ?? "Provider",
                subtotal = i.Subtotal,
                taxAmount = i.TaxAmount,
                totalAmount = i.TotalAmount ?? (i.Subtotal + i.TaxAmount),
                amountPaid = i.AmountPaid,
                currency = i.Currency,
                status = i.Status,
                issuedDate = i.IssuedDate?.ToString("yyyy-MM-dd") ?? i.CreatedDate.ToString("yyyy-MM-dd"),
                dueDate = i.DueDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                paidDate = i.PaidDate?.ToString("yyyy-MM-dd"),
                documentUrl = i.DocumentUrl,
                lines = i.InvoiceLines.Select(l => new InvoiceLineDto
                {
                    id = l.InvoiceLineId,
                    invoiceId = l.InvoiceId,
                    description = l.Description,
                    quantity = (int)l.Quantity,
                    unitPrice = l.UnitPrice,
                    lineTotal = l.LineTotal ?? (l.Quantity * l.UnitPrice),
                    itemType = "Item",
                }).ToList(),
            }).ToList();
        }

        public async Task<List<PaymentDto>> GetPaymentsService(Guid userId, string role)
        {
            var query = _context.Payments
                .Include(p => p.Invoice)
                .Include(p => p.Booking)
                .AsQueryable();

            if (role == "Advertiser")
            {
                var adv = await _context.Advertisers.FirstOrDefaultAsync(a => a.UserId == userId);
                if (adv == null) return new List<PaymentDto>();
                query = query.Where(p => p.Booking.AdvertiserId == adv.AdvertiserId);
            }
            else if (role == "Provider")
            {
                var prov = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);
                if (prov == null) return new List<PaymentDto>();
                query = query.Where(p => p.Booking.ProviderId == prov.ProviderId);
            }

            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return payments.Select(p => new PaymentDto
            {
                id = p.PaymentId,
                paymentNumber = $"PAY-{p.PaymentId.ToString("N")[..8].ToUpper()}",
                invoiceId = p.InvoiceId ?? Guid.Empty,
                invoiceNumber = p.Invoice?.InvoiceNumber ?? string.Empty,
                bookingId = p.BookingId,
                bookingNumber = p.Booking?.BookingNumber ?? string.Empty,
                amount = p.Amount,
                currency = p.Currency,
                paymentMethod = p.PaymentMethod ?? string.Empty,
                transactionReference = p.TransactionReference ?? string.Empty,
                status = p.Status,
                paymentDate = p.PaymentDate?.ToString("yyyy-MM-dd HH:mm") ?? p.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
                notes = p.Gateway,
            }).ToList();
        }

        public async Task<List<CommissionRuleDto>> GetCommissionRulesService()
        {
            var rules = await _context.CommissionRules
                .Include(r => r.Channel)
                .Include(r => r.ProviderType)
                .OrderBy(r => r.Priority)
                .ToListAsync();

            return rules.Select(r => new CommissionRuleDto
            {
                id = r.CommissionRuleId,
                scope = r.Scope,
                channelId = r.ChannelId,
                channelName = r.Channel?.ChannelName,
                providerTypeId = r.ProviderTypeId,
                providerTypeName = r.ProviderType?.Name,
                percentageRate = r.PercentageRate,
                fixedAmount = r.FixedAmount,
                minFee = r.MinFee,
                maxFee = r.MaxFee,
                currency = r.Currency,
                priority = r.Priority,
                isActive = r.IsActive,
                effectiveFrom = r.EffectiveFrom.ToString("yyyy-MM-dd"),
                effectiveTo = r.EffectiveTo?.ToString("yyyy-MM-dd"),
            }).ToList();
        }

        public async Task<(bool created, string message, CommissionRuleDto? rule)> CreateCommissionRuleService(Guid userId, CreateCommissionRuleDto dto)
        {
            Guid? channelId = dto.channelId;
            if (!channelId.HasValue && !string.IsNullOrWhiteSpace(dto.channelName))
            {
                var ch = await _context.AdvertisingChannels.FirstOrDefaultAsync(c => c.ChannelName == dto.channelName.Trim());
                channelId = ch?.ChannelId;
            }

            Guid? providerTypeId = dto.providerTypeId;
            if (!providerTypeId.HasValue && !string.IsNullOrWhiteSpace(dto.providerTypeName))
            {
                var pt = await _context.ProviderTypes.FirstOrDefaultAsync(p => p.Name == dto.providerTypeName.Trim());
                providerTypeId = pt?.ProviderTypeId;
            }

            var rule = new CommissionRule
            {
                CommissionRuleId = Guid.NewGuid(),
                Scope = dto.scope,
                ChannelId = channelId,
                ProviderTypeId = providerTypeId,
                PercentageRate = dto.percentageRate,
                FixedAmount = dto.fixedAmount,
                MinFee = dto.minFee,
                MaxFee = dto.maxFee,
                Currency = string.IsNullOrWhiteSpace(dto.currency) ? "INR" : dto.currency,
                Priority = dto.priority,
                IsActive = true,
                CreatedBy = userId,
                EffectiveFrom = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
            };

            _context.CommissionRules.Add(rule);

            _context.AuditLogs.Add(new AuditLog
            {
                AuditLogId = Guid.NewGuid(),
                UserId = userId,
                UserRole = "Admin",
                EntityName = "CommissionRule",
                EntityId = rule.CommissionRuleId,
                Action = "CREATE",
                OldValues = null,
                NewValues = JsonSerializer.Serialize(new { scope = rule.Scope, percentageRate = rule.PercentageRate, fixedAmount = rule.FixedAmount, priority = rule.Priority, isActive = rule.IsActive }),
                // No request context reaches this layer, so the caller's IP and user
                // agent are unknown; they are left empty rather than invented.
                IpAddress = null,
                UserAgent = null,
                CreatedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var chObj = rule.ChannelId.HasValue ? await _context.AdvertisingChannels.FindAsync(rule.ChannelId) : null;
            var ptObj = rule.ProviderTypeId.HasValue ? await _context.ProviderTypes.FindAsync(rule.ProviderTypeId) : null;

            return (true, "Commission rule created successfully", new CommissionRuleDto
            {
                id = rule.CommissionRuleId,
                scope = rule.Scope,
                channelId = rule.ChannelId,
                channelName = chObj?.ChannelName,
                providerTypeId = rule.ProviderTypeId,
                providerTypeName = ptObj?.Name,
                percentageRate = rule.PercentageRate,
                fixedAmount = rule.FixedAmount,
                currency = rule.Currency,
                priority = rule.Priority,
                isActive = rule.IsActive,
                effectiveFrom = rule.EffectiveFrom.ToString("yyyy-MM-dd"),
            });
        }

        // There is no payment gateway integration yet, so an invoice cannot be paid
        // online. Refusing here (rather than marking the invoice Paid and inventing a
        // Payment row and transaction reference) keeps the ledger to real money only.
        public Task<(bool paid, string message, PaymentDto? payment)> PayInvoiceService(Guid userId, PayInvoiceDto dto) =>
            Task.FromResult<(bool paid, string message, PaymentDto? payment)>((false, "Online payment is not available yet", null));
    }
}
