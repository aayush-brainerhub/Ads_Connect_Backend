namespace AdsConnect.data.Dtos
{
    public class CampaignDto
    {
        public Guid id { get; set; }
        public string title { get; set; } = string.Empty;
        public string? industry { get; set; }

        public string? location { get; set; }

        public decimal budget { get; set; }
        public string objective { get; set; } = string.Empty;
        public string? description { get; set; }

        public string status { get; set; } = string.Empty;

        public int responses { get; set; }

        public string createdAt { get; set; } = string.Empty;

        public string? advertiser { get; set; }
    }

    public class CreateCampaignDto
    {
        public string title { get; set; } = string.Empty;
        public Guid? industryId { get; set; }
        public Guid? locationId { get; set; }
        public decimal budget { get; set; }

        public string objective { get; set; } = string.Empty;

        public string? description { get; set; }
    }

    public class UpdateCampaignDto : CreateCampaignDto
    {
        public Guid id { get; set; }
    }
}
