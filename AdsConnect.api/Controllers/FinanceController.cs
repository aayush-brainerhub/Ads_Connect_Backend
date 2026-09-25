using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdsConnect.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FinanceController : ApiControllerBase
    {
        private readonly IFinanceService _financeService;

        public FinanceController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        [HttpGet("GetOverview")]
        [Authorize(Roles = "Admin")]
        public Task<ActionResult> GetOverview() =>
            ForCurrentUser<FinancialOverviewDto>(async _ =>
                Success(await _financeService.GetFinancialOverviewService(), "financial overview"));

        [HttpGet("GetInvoices")]
        public Task<ActionResult> GetInvoices() =>
            ForCurrentUser<List<InvoiceDto>>(async userId =>
            {
                var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";
                return Success(await _financeService.GetInvoicesService(userId, role), "invoices");
            });

        [HttpGet("GetPayments")]
        public Task<ActionResult> GetPayments() =>
            ForCurrentUser<List<PaymentDto>>(async userId =>
            {
                var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";
                return Success(await _financeService.GetPaymentsService(userId, role), "payments");
            });

        [HttpGet("GetCommissionRules")]
        [Authorize(Roles = "Admin")]
        public Task<ActionResult> GetCommissionRules() =>
            ForCurrentUser<List<CommissionRuleDto>>(async _ =>
                Success(await _financeService.GetCommissionRulesService(), "commission rules"));

        [HttpPost("CreateCommissionRule")]
        [Authorize(Roles = "Admin")]
        public Task<ActionResult> CreateCommissionRule(CreateCommissionRuleDto dto) =>
            ForCurrentUser<CommissionRuleDto>(async userId =>
            {
                var (created, message, rule) = await _financeService.CreateCommissionRuleService(userId, dto);
                return created ? Success(rule!, message) : Failure<CommissionRuleDto>(message);
            });

        [HttpPost("PayInvoice")]
        [Authorize(Roles = "Advertiser")]
        public Task<ActionResult> PayInvoice(PayInvoiceDto dto) =>
            ForCurrentUser<PaymentDto>(async userId =>
            {
                var (paid, message, payment) = await _financeService.PayInvoiceService(userId, dto);
                return paid ? Success(payment!, message) : Failure<PaymentDto>(message);
            });
    }
}
