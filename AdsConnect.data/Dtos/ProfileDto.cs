namespace AdsConnect.data.Dtos
{
    public class MyProfileDto
    {
        public AuthUserDto? user { get; set; }

        public AdvertiserProfileDto? advertiser { get; set; }
        public ProviderProfileDto? provider { get; set; }
    }

    public class AdvertiserProfileDto
    {
        public Guid id { get; set; }
        public string businessName { get; set; } = string.Empty;
        public Guid? industryId { get; set; }
        public string? industry { get; set; }
        public Guid? locationId { get; set; }
        public string? location { get; set; }
        public string? website { get; set; }
        public decimal? monthlyBudgetMin { get; set; }
        public decimal? monthlyBudgetMax { get; set; }
    }

    public class SaveAdvertiserProfileDto
    {
        public string businessName { get; set; } = string.Empty;
        public Guid? industryId { get; set; }
        public Guid? locationId { get; set; }
        public string? website { get; set; }
        public decimal? monthlyBudgetMin { get; set; }
        public decimal? monthlyBudgetMax { get; set; }
    }

    public class ProviderProfileDto
    {
        public Guid id { get; set; }
        public string providerName { get; set; } = string.Empty;
        public Guid providerTypeId { get; set; }
        public string providerType { get; set; } = string.Empty;
        public Guid? locationId { get; set; }
        public string? location { get; set; }
        public string? phoneNumber { get; set; }
        public string? website { get; set; }
        public string? description { get; set; }
        public long totalAudience { get; set; }
        public Guid? primaryChannelId { get; set; }
        public string? primaryChannel { get; set; }
    }

    public class SaveProviderProfileDto
    {
        public string providerName { get; set; } = string.Empty;
        public Guid providerTypeId { get; set; }
        public Guid? locationId { get; set; }
        public Guid? primaryChannelId { get; set; }

        public long? audienceSize { get; set; }
        public string? phoneNumber { get; set; }
        public string? website { get; set; }
        public string? description { get; set; }
    }
}
