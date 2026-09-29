using System.Net;
using System.Text.Json;
using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using AdsConnect.data.Model;
using Microsoft.EntityFrameworkCore;

namespace AdsConnect.data.Repository
{
    public class AuditService : IAuditService
    {
        private readonly AdsConnectContext _context;

        public AuditService(AdsConnectContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLogDto>> GetAuditLogsService()
        {
            var logs = await _context.AuditLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedDate)
                .Take(200)
                .ToListAsync();

            return logs.Select(a => new AuditLogDto
            {
                id = a.AuditLogId,
                userId = a.UserId,
                userName = a.User == null ? string.Empty : $"{a.User.FirstName} {a.User.LastName}".Trim(),
                userEmail = a.User?.Email ?? string.Empty,
                userRole = a.UserRole ?? "System",
                entityName = a.EntityName,
                entityId = a.EntityId,
                action = a.Action,
                oldValues = a.OldValues,
                newValues = a.NewValues,
                ipAddress = a.IpAddress?.ToString() ?? string.Empty,
                userAgent = a.UserAgent ?? string.Empty,
                createdDate = a.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
            }).ToList();
        }

        public async Task LogActionAsync(
            Guid? userId,
            string? userRole,
            string entityName,
            Guid? entityId,
            string action,
            object? oldValues,
            object? newValues,
            string? ipAddress,
            string? userAgent)
        {
            try
            {
                IPAddress? ip = null;
                if (!string.IsNullOrEmpty(ipAddress) && IPAddress.TryParse(ipAddress, out var parsedIp))
                {
                    ip = parsedIp;
                }

                var log = new AuditLog
                {
                    AuditLogId = Guid.NewGuid(),
                    UserId = userId,
                    UserRole = userRole,
                    EntityName = entityName,
                    EntityId = entityId,
                    Action = action,
                    OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                    NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                    IpAddress = ip,
                    UserAgent = userAgent,
                    CreatedDate = DateTime.UtcNow,
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Audit logging failure should not break primary operation flow
            }
        }
    }
}
