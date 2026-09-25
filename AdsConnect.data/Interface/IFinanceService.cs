using AdsConnect.data.Dtos;

namespace AdsConnect.data.Interface
{
    public interface IFinanceService
    {
        Task<FinancialOverviewDto> GetFinancialOverviewService();
        Task<List<InvoiceDto>> GetInvoicesService(Guid userId, string role);
        Task<List<PaymentDto>> GetPaymentsService(Guid userId, string role);
        Task<List<CommissionRuleDto>> GetCommissionRulesService();
        Task<(bool created, string message, CommissionRuleDto? rule)> CreateCommissionRuleService(Guid userId, CreateCommissionRuleDto dto);
        Task<(bool paid, string message, PaymentDto? payment)> PayInvoiceService(Guid userId, PayInvoiceDto dto);
    }
}
