using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoyaltyRewardsApi.Models
{
    public class RewardsCatalog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PointsRequired { get; set; }

        public int StockQuantity { get; set; } = -1; // -1 means unlimited

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Redemption> Redemptions { get; set; } = new List<Redemption>();

        [NotMapped]
        public bool IsInStock => StockQuantity == -1 || StockQuantity > 0;

        [NotMapped]
        public bool IsAvailable => IsActive && IsInStock;
    }
}
