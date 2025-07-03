using LoyaltyRewardsApi.Models;
using System.ComponentModel.DataAnnotations;

namespace LoyaltyRewardsApi.DTOs
{
    public class RewardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public decimal PointsRequired { get; set; }
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsInStock { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateRewardDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Points required must be greater than 0")]
        public decimal PointsRequired { get; set; }

        public int StockQuantity { get; set; } = -1;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateRewardDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? PointsRequired { get; set; }

        public int? StockQuantity { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public bool? IsActive { get; set; }
    }

    public class RedeemRewardRequestDto
    {
        [Required]
        public int RewardId { get; set; }

        public string? Notes { get; set; }
    }

    public class RedemptionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int RewardId { get; set; }
        public string RewardName { get; set; } = string.Empty;
        public decimal PointsUsed { get; set; }
        public RedemptionStatus Status { get; set; }
        public string? RedemptionCode { get; set; }
        public string? Notes { get; set; }
        public int? ProcessedBy { get; set; }
        public string? ProcessedByName { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProcessRedemptionDto
    {
        [Required]
        public RedemptionStatus Status { get; set; }

        public string? Notes { get; set; }

        public string? RedemptionCode { get; set; }
    }

    public class RedemptionHistoryDto
    {
        public List<RedemptionDto> Redemptions { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class RewardCategoryDto
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal MinPoints { get; set; }
        public decimal MaxPoints { get; set; }
    }
}
