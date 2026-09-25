namespace AdsConnect.data.Dtos
{
    public class InventoryDto
    {
        public Guid id { get; set; }
        public string name { get; set; } = string.Empty;
        public string? description { get; set; }

        public Guid channelId { get; set; }
        public string channel { get; set; } = string.Empty;

        public decimal price { get; set; }
        public Guid? pricingUnitId { get; set; }
        public string? pricingUnit { get; set; }

        public string status { get; set; } = string.Empty;
    }

    public class SaveInventoryDto
    {
        public Guid? id { get; set; }

        public string name { get; set; } = string.Empty;
        public string? description { get; set; }
        public Guid channelId { get; set; }
        public decimal price { get; set; }
        public Guid pricingUnitId { get; set; }
        public string status { get; set; } = "Active";
    }
}
