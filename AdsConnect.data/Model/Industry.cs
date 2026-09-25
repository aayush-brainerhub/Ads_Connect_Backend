using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class Industry
{
    public Guid IndustryId { get; set; }

    public string IndustryName { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<Advertiser> Advertisers { get; set; } = new List<Advertiser>();

    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
}
