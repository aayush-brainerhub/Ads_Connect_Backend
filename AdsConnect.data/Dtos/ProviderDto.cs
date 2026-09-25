namespace AdsConnect.data.Dtos
{
    public class ProviderDto
    {
        public Guid id { get; set; }
        public string name { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;

        public string? location { get; set; }

        public string? category { get; set; }

        public decimal price { get; set; }

        public long audience { get; set; }
        public decimal rating { get; set; }
        public string? description { get; set; }
    }
}
