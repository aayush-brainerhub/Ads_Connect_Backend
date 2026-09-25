namespace AdsConnect.data.Dtos
{
    public class ProviderRequestDto
    {
        public Guid id { get; set; }
        public string campaignTitle { get; set; } = string.Empty;
        public string advertiser { get; set; } = string.Empty;
        public decimal budget { get; set; }

        public string status { get; set; } = string.Empty;

        public string date { get; set; } = string.Empty;
        public string? message { get; set; }

        public Guid? conversationId { get; set; }
    }

    public class CreateRequestDto
    {
        public Guid campaignId { get; set; }
        public Guid providerId { get; set; }

        public decimal budget { get; set; }

        public string? message { get; set; }
    }

    public class RespondToRequestDto
    {
        public Guid requestId { get; set; }

        public string status { get; set; } = string.Empty;

        public string? declineReason { get; set; }
    }
}
