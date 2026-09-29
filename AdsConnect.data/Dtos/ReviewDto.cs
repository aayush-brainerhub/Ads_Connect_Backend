using System.ComponentModel.DataAnnotations;

namespace AdsConnect.data.Dtos
{
    public class CriteriaRatingsDto
    {
        public int communication { get; set; }
        public int deliverySpeed { get; set; }
        public int quality { get; set; }
        public int valueForMoney { get; set; }
    }

    public class ReviewDto
    {
        public Guid id { get; set; }
        public Guid bookingId { get; set; }
        public string bookingNumber { get; set; } = string.Empty;
        public string campaignTitle { get; set; } = string.Empty;
        public string direction { get; set; } = string.Empty;
        public string reviewerName { get; set; } = string.Empty;
        public string reviewerRole { get; set; } = string.Empty;
        public Guid? providerId { get; set; }
        public string providerName { get; set; } = string.Empty;
        public Guid? advertiserId { get; set; }
        public string advertiserName { get; set; } = string.Empty;
        public short rating { get; set; }
        public string title { get; set; } = string.Empty;
        public string reviewText { get; set; } = string.Empty;
        public CriteriaRatingsDto? criteriaRatings { get; set; }
        public bool isPublished { get; set; }
        public string? responseText { get; set; }
        public string? responseDate { get; set; }
        public string createdDate { get; set; } = string.Empty;
    }

    public class CreateReviewDto
    {
        public Guid? bookingId { get; set; }

        public Guid? providerId { get; set; }

        [Range(1, 5)]
        public short rating { get; set; } = 5;

        [Required]
        public string title { get; set; } = string.Empty;

        [Required]
        public string reviewText { get; set; } = string.Empty;

        public CriteriaRatingsDto? criteriaRatings { get; set; }
    }

    public class ReplyToReviewDto
    {
        [Required]
        public Guid reviewId { get; set; }

        [Required]
        public string responseText { get; set; } = string.Empty;
    }
}
