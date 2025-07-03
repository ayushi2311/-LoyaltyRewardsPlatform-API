using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoyaltyRewardsApi.Models
{
    public class PointTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? AppId { get; set; }

        [Required]
        public TransactionType TransactionType { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Points { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? ReferenceId { get; set; }

        [StringLength(100)]
        public string? ExternalTransactionId { get; set; }

        public TransactionStatus Status { get; set; } = TransactionStatus.Completed;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("AppId")]
        public virtual ThirdPartyApp? App { get; set; }
    }

    public enum TransactionType
    {
        Earned,
        Redeemed,
        Adjusted
    }

    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed,
        Cancelled
    }
}
