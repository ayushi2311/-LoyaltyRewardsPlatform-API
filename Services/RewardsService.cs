using LoyaltyRewardsApi.Data;
using LoyaltyRewardsApi.DTOs;
using LoyaltyRewardsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyRewardsApi.Services
{
    public class RewardsService : IRewardsService
    {
        private readonly LoyaltyRewardsContext _context;

        public RewardsService(LoyaltyRewardsContext context)
        {
            _context = context;
        }

        public async Task<RewardDto> CreateReward(CreateRewardDto dto)
        {
            var reward = new RewardsCatalog
            {
                Name = dto.Name,
                Description = dto.Description,
                Category = dto.Category,
                PointsRequired = dto.PointsRequired,
                StockQuantity = dto.StockQuantity,
                ImageUrl = dto.ImageUrl,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.RewardsCatalog.Add(reward);
            await _context.SaveChangesAsync();

            return new RewardDto
            {
                Id = reward.Id,
                Name = reward.Name,
                Description = reward.Description,
                Category = reward.Category,
                PointsRequired = reward.PointsRequired,
                StockQuantity = reward.StockQuantity,
                ImageUrl = reward.ImageUrl,
                IsActive = reward.IsActive,
                IsInStock = reward.IsInStock,
                IsAvailable = reward.IsAvailable,
                CreatedAt = reward.CreatedAt,
                UpdatedAt = reward.UpdatedAt
            };
        }

        public async Task<RewardDto> UpdateReward(int id, UpdateRewardDto dto)
        {
            var reward = await _context.RewardsCatalog.FindAsync(id);

            if (reward == null)
                throw new ArgumentException("Reward not found.");

            reward.Name = dto.Name ?? reward.Name;
            reward.Description = dto.Description;
            reward.Category = dto.Category;
            reward.PointsRequired = dto.PointsRequired ?? reward.PointsRequired;
            reward.StockQuantity = dto.StockQuantity ?? reward.StockQuantity;
            reward.ImageUrl = dto.ImageUrl;
            reward.IsActive = dto.IsActive ?? reward.IsActive;
            reward.UpdatedAt = DateTime.UtcNow;

            _context.RewardsCatalog.Update(reward);
            await _context.SaveChangesAsync();

            return new RewardDto
            {
                Id = reward.Id,
                Name = reward.Name,
                Description = reward.Description,
                Category = reward.Category,
                PointsRequired = reward.PointsRequired,
                StockQuantity = reward.StockQuantity,
                ImageUrl = reward.ImageUrl,
                IsActive = reward.IsActive,
                IsInStock = reward.IsInStock,
                IsAvailable = reward.IsAvailable,
                CreatedAt = reward.CreatedAt,
                UpdatedAt = reward.UpdatedAt
            };
        }

        public async Task DeleteReward(int id)
        {
            var reward = await _context.RewardsCatalog.FindAsync(id);

            if (reward == null)
                throw new ArgumentException("Reward not found.");

            _context.RewardsCatalog.Remove(reward);
            await _context.SaveChangesAsync();
        }

        public async Task<RewardDto> GetRewardById(int id)
        {
            var reward = await _context.RewardsCatalog.FindAsync(id);

            if (reward == null)
                throw new ArgumentException("Reward not found.");

            return new RewardDto
            {
                Id = reward.Id,
                Name = reward.Name,
                Description = reward.Description,
                Category = reward.Category,
                PointsRequired = reward.PointsRequired,
                StockQuantity = reward.StockQuantity,
                ImageUrl = reward.ImageUrl,
                IsActive = reward.IsActive,
                IsInStock = reward.IsInStock,
                IsAvailable = reward.IsAvailable,
                CreatedAt = reward.CreatedAt,
                UpdatedAt = reward.UpdatedAt
            };
        }

        public async Task<List<RewardDto>> GetAllRewards()
        {
            return await _context.RewardsCatalog
                .Select(reward => new RewardDto
                {
                    Id = reward.Id,
                    Name = reward.Name,
                    Description = reward.Description,
                    Category = reward.Category,
                    PointsRequired = reward.PointsRequired,
                    StockQuantity = reward.StockQuantity,
                    ImageUrl = reward.ImageUrl,
                    IsActive = reward.IsActive,
                    IsInStock = reward.IsInStock,
                    IsAvailable = reward.IsAvailable,
                    CreatedAt = reward.CreatedAt,
                    UpdatedAt = reward.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<RedemptionDto> RedeemReward(int userId, RedeemRewardRequestDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.Include(u => u.Wallet).FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    throw new ArgumentException("User not found.");

                var reward = await _context.RewardsCatalog.FirstOrDefaultAsync(r => r.Id == dto.RewardId && r.IsActive && r.IsInStock);
                if (reward == null)
                    throw new ArgumentException("Reward not available.");

                if (user.Wallet == null || user.Wallet.Balance < reward.PointsRequired)
                    throw new ArgumentException("Insufficient points.");

                // Deduct points and reduce stock
                user.Wallet.Balance -= reward.PointsRequired;
                if (reward.StockQuantity > 0)
                {
                    reward.StockQuantity--;
                }

                // Create redemption
                var redemption = new Redemption
                {
                    UserId = userId,
                    RewardId = reward.Id,
                    PointsUsed = reward.PointsRequired,
                    Status = RedemptionStatus.Pending,
                    RedemptionCode = Guid.NewGuid().ToString(),
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Redemptions.Add(redemption);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new RedemptionDto
                {
                    Id = redemption.Id,
                    UserId = redemption.UserId,
                    UserName = user.FullName,
                    RewardId = reward.Id,
                    RewardName = reward.Name,
                    PointsUsed = redemption.PointsUsed,
                    Status = redemption.Status,
                    RedemptionCode = redemption.RedemptionCode,
                    Notes = redemption.Notes,
                    CreatedAt = redemption.CreatedAt
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<RedemptionDto> ProcessRedemption(int redemptionId, ProcessRedemptionDto dto, int processedBy)
        {
            var redemption = await _context.Redemptions.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == redemptionId);
            if (redemption == null)
                throw new ArgumentException("Redemption not found.");

            redemption.Status = dto.Status;
            redemption.Notes = dto.Notes;
            redemption.RedemptionCode = dto.RedemptionCode ?? redemption.RedemptionCode;
            redemption.ProcessedBy = processedBy;
            redemption.ProcessedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new RedemptionDto
            {
                Id = redemption.Id,
                UserId = redemption.UserId,
                UserName = redemption.User != null ? redemption.User.FullName : "",
                RewardId = redemption.RewardId,
                PointsUsed = redemption.PointsUsed,
                Status = redemption.Status,
                RedemptionCode = redemption.RedemptionCode,
                Notes = redemption.Notes,
                ProcessedBy = redemption.ProcessedBy,
                ProcessedAt = redemption.ProcessedAt,
                CreatedAt = redemption.CreatedAt
            };
        }

        public async Task<RedemptionHistoryDto> GetRedemptionHistory(int userId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Redemptions
                .Include(r => r.User)
                .Include(r => r.Reward)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var redemptions = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RedemptionDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = r.User.FirstName + " " + r.User.LastName,
                    RewardId = r.RewardId,
                    RewardName = r.Reward != null ? r.Reward.Name : "",
                    PointsUsed = r.PointsUsed,
                    Status = r.Status,
                    RedemptionCode = r.RedemptionCode,
                    Notes = r.Notes,
                    ProcessedBy = r.ProcessedBy,
                    ProcessedAt = r.ProcessedAt,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return new RedemptionHistoryDto
            {
                Redemptions = redemptions,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<RedemptionHistoryDto> GetAllRedemptions(int pageNumber = 1, int pageSize = 20)
        {
            var query = _context.Redemptions
                .Include(r => r.User)
                .Include(r => r.Reward)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var redemptions = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RedemptionDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = r.User.FirstName + " " + r.User.LastName,
                    RewardId = r.RewardId,
                    RewardName = r.Reward != null ? r.Reward.Name : "",
                    PointsUsed = r.PointsUsed,
                    Status = r.Status,
                    RedemptionCode = r.RedemptionCode,
                    Notes = r.Notes,
                    ProcessedBy = r.ProcessedBy,
                    ProcessedAt = r.ProcessedAt,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return new RedemptionHistoryDto
            {
                Redemptions = redemptions,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }
    }

    public interface IRewardsService
    {
        Task<RewardDto> CreateReward(CreateRewardDto dto);
        Task<RewardDto> UpdateReward(int id, UpdateRewardDto dto);
        Task DeleteReward(int id);
        Task<RewardDto> GetRewardById(int id);
        Task<List<RewardDto>> GetAllRewards();
        Task<RedemptionDto> RedeemReward(int userId, RedeemRewardRequestDto dto);
        Task<RedemptionDto> ProcessRedemption(int redemptionId, ProcessRedemptionDto dto, int processedBy);
        Task<RedemptionHistoryDto> GetRedemptionHistory(int userId, int pageNumber = 1, int pageSize = 10);
        Task<RedemptionHistoryDto> GetAllRedemptions(int pageNumber = 1, int pageSize = 20);
    }
}
