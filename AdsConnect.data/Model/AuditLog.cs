using System;
using System.Collections.Generic;
using System.Net;

namespace AdsConnect.data.Model;

public partial class AuditLog
{
    public Guid AuditLogId { get; set; }

    public Guid? UserId { get; set; }

    public string? UserRole { get; set; }

    public string EntityName { get; set; } = null!;

    public Guid? EntityId { get; set; }

    public string Action { get; set; } = null!;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public IPAddress? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? RequestId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual AppUser? User { get; set; }
}
