using System;
using System.Collections.Generic;

namespace AdsConnect.data.Model;

public partial class Review
{
    public Guid ReviewId { get; set; }

    public Guid BookingId { get; set; }

    public string Direction { get; set; } = null!;

    public Guid? ReviewerUserId { get; set; }

    public Guid? ProviderId { get; set; }

    public Guid? AdvertiserId { get; set; }

    public short Rating { get; set; }

    public string? Title { get; set; }

    public string? ReviewText { get; set; }

    public string CriteriaRatings { get; set; } = null!;

    public bool IsPublished { get; set; }

    public string? ResponseText { get; set; }

    public DateTime? ResponseDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Advertiser? Advertiser { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Provider? Provider { get; set; }

    public virtual AppUser? ReviewerUser { get; set; }
}
