using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsConnect.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AuditController : ApiControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet("GetAuditLogs")]
        public Task<ActionResult> GetAuditLogs() =>
            ForCurrentUser<List<AuditLogDto>>(async _ =>
                Success(await _auditService.GetAuditLogsService(), "audit logs"));
    }
}
