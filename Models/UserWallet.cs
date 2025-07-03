using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoyaltyRewardsApi.Models
{
    public class UserWallet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Balance { get; set; } = 0.00m;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalEarned { get; set; } = 0.00m;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalRedeemed { get; set; } = 0.00m;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
