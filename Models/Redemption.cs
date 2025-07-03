using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoyaltyRewardsApi.Models
{
    public class Redemption
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int RewardId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PointsUsed { get; set; }

        public RedemptionStatus Status { get; set; } = RedemptionStatus.Pending;

        [StringLength(50)]
        public string? RedemptionCode { get; set; }

        public string? Notes { get; set; }

        public int? ProcessedBy { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("RewardId")]
        public virtual RewardsCatalog Reward { get; set; } = null!;

        [ForeignKey("ProcessedBy")]
        public virtual User? ProcessedByUser { get; set; }
    }

    public enum RedemptionStatus
    {
        Pending,
        Approved,
        Delivered,
        Cancelled
    }
}
