namespace AdsConnect.data.Dtos
{
    public class AdminMetricsDto
    {
        public int totalProviders { get; set; }
        public int totalAdvertisers { get; set; }
        public int totalCampaigns { get; set; }

        public decimal committedBudget { get; set; }

        public int totalUsers { get; set; }
        public int activeCampaigns { get; set; }
    }

    public class AdminUserDto
    {
        public Guid id { get; set; }
        public string name { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;

        public string joined { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
    }
}
