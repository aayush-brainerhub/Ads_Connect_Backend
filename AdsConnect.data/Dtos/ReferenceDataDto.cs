namespace AdsConnect.data.Dtos
{
    public class IndustryDto
    {
        public Guid id { get; set; }
        public string name { get; set; } = string.Empty;
    }

    public class LocationDto
    {
        public Guid id { get; set; }
        public string city { get; set; } = string.Empty;
        public string? state { get; set; }
    }

    public class ProviderTypeDto
    {
        public Guid id { get; set; }
        public string name { get; set; } = string.Empty;
    }

    public class ChannelDto
    {
        public Guid id { get; set; }
        public string name { get; set; } = string.Empty;
        public string category { get; set; } = string.Empty;
        public string? description { get; set; }
    }

    public class PricingUnitDto
    {
        public Guid id { get; set; }

        public string code { get; set; } = string.Empty;

        public string name { get; set; } = string.Empty;
    }

    public class ReferenceDataDto
    {
        public List<IndustryDto> industries { get; set; } = new();
        public List<LocationDto> locations { get; set; } = new();
        public List<ProviderTypeDto> providerTypes { get; set; } = new();
        public List<ChannelDto> channels { get; set; } = new();
        public List<PricingUnitDto> pricingUnits { get; set; } = new();

        public List<string> objectives { get; set; } = new();
    }

    public class LookupItemDto
    {
        public Guid id { get; set; }
        public string name { get; set; } = string.Empty;
        public string? code { get; set; }
        public string? category { get; set; }
        public string? description { get; set; }
        public bool isActive { get; set; }
        public int itemCount { get; set; }
    }

    public class LookupsBundleDto
    {
        public List<LookupItemDto> channels { get; set; } = new();
        public List<LookupItemDto> industries { get; set; } = new();
        public List<LookupItemDto> locations { get; set; } = new();
        public List<LookupItemDto> providerTypes { get; set; } = new();
        public List<LookupItemDto> pricingUnits { get; set; } = new();
        public List<LookupItemDto> roles { get; set; } = new();
    }

    public class CreateLookupItemDto
    {
        public string category { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string? code { get; set; }
        public string? subCategory { get; set; }
        public string? description { get; set; }
        public string? state { get; set; }
    }

    public class ToggleLookupStatusDto
    {
        public string category { get; set; } = string.Empty;
        public Guid id { get; set; }
    }
}
