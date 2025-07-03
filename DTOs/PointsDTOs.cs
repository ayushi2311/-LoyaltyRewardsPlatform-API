using LoyaltyRewardsApi.Models;
using System.ComponentModel.DataAnnotations;

namespace LoyaltyRewardsApi.DTOs
{
    public class AddPointsRequestDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Points must be greater than 0")]
        public decimal Points { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? ReferenceId { get; set; }

        [StringLength(100)]
        public string? ExternalTransactionId { get; set; }

        public int? AppId { get; set; }
    }

    public class PointTransactionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int? AppId { get; set; }
        public string? AppName { get; set; }
        public TransactionType TransactionType { get; set; }
        public decimal Points { get; set; }
        public string? Description { get; set; }
        public string? ReferenceId { get; set; }
        public string? ExternalTransactionId { get; set; }
        public TransactionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TransactionHistoryDto
    {
        public List<PointTransactionDto> Transactions { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class UserWalletSummaryDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public decimal TotalEarned { get; set; }
        public decimal TotalRedeemed { get; set; }
        public List<PointTransactionDto> RecentTransactions { get; set; } = new();
    }

    public class BulkPointsRequestDto
    {
        [Required]
        public List<UserPointsDto> UserPoints { get; set; } = new();

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? ReferenceId { get; set; }

        public int? AppId { get; set; }
    }

    public class UserPointsDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Points { get; set; }
    }
}
