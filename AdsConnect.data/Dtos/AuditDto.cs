namespace AdsConnect.data.Dtos
{
    public class AuditLogDto
    {
        public Guid id { get; set; }
        public Guid? userId { get; set; }
        public string userName { get; set; } = string.Empty;
        public string userEmail { get; set; } = string.Empty;
        public string userRole { get; set; } = string.Empty;
        public string entityName { get; set; } = string.Empty;
        public Guid? entityId { get; set; }
        public string action { get; set; } = string.Empty;
        public string? oldValues { get; set; }
        public string? newValues { get; set; }
        public string ipAddress { get; set; } = string.Empty;
        public string userAgent { get; set; } = string.Empty;
        public string createdDate { get; set; } = string.Empty;
    }
}
