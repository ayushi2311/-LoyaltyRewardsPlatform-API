using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoyaltyRewardsApi.Models
{
    public class ThirdPartyApp
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string AppName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string ApiKey { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string ApiSecret { get; set; } = string.Empty;

        [StringLength(500)]
        public string? WebhookUrl { get; set; }

        public bool IsActive { get; set; } = true;

        [Column(TypeName = "decimal(5,2)")]
        public decimal PointsMultiplier { get; set; } = 1.00m;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<PointTransaction> Transactions { get; set; } = new List<PointTransaction>();
    }
}
