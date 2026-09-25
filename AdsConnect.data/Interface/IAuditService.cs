using AdsConnect.data.Dtos;

namespace AdsConnect.data.Interface
{
    public interface IAuditService
    {
        Task<List<AuditLogDto>> GetAuditLogsService();
        Task LogActionAsync(Guid? userId, string? userRole, string entityName, Guid? entityId, string action, object? oldValues, object? newValues, string? ipAddress, string? userAgent);
    }
}
