using LoyaltyRewardsApi.Data;
using LoyaltyRewardsApi.DTOs;
using LoyaltyRewardsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyRewardsApi.Services
{
    public class PointsService : IPointsService
    {
        private readonly LoyaltyRewardsContext _context;

        public PointsService(LoyaltyRewardsContext context)
        {
            _context = context;
        }

        public async Task<PointTransactionDto> AddPoints(AddPointsRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Verify user exists
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                    throw new ArgumentException("User not found");

                // Verify app exists if provided
                ThirdPartyApp? app = null;
                if (request.AppId.HasValue)
                {
                    app = await _context.ThirdPartyApps.FindAsync(request.AppId.Value);
                    if (app == null)
                        throw new ArgumentException("Third party app not found");
                }

                // Calculate final points with multiplier
                var finalPoints = request.Points;
                if (app != null)
                {
                    finalPoints *= app.PointsMultiplier;
                }

                // Create transaction
                var pointTransaction = new PointTransaction
                {
                    UserId = request.UserId,
                    AppId = request.AppId,
                    TransactionType = TransactionType.Earned,
                    Points = finalPoints,
                    Description = request.Description,
                    ReferenceId = request.ReferenceId,
                    ExternalTransactionId = request.ExternalTransactionId,
                    Status = TransactionStatus.Completed,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PointTransactions.Add(pointTransaction);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new PointTransactionDto
                {
                    Id = pointTransaction.Id,
                    UserId = pointTransaction.UserId,
                    UserName = user.FullName,
                    AppId = pointTransaction.AppId,
                    AppName = app?.AppName,
                    TransactionType = pointTransaction.TransactionType,
                    Points = pointTransaction.Points,
                    Description = pointTransaction.Description,
                    ReferenceId = pointTransaction.ReferenceId,
                    ExternalTransactionId = pointTransaction.ExternalTransactionId,
                    Status = pointTransaction.Status,
                    CreatedAt = pointTransaction.CreatedAt
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<PointTransactionDto>> AddBulkPoints(BulkPointsRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var results = new List<PointTransactionDto>();

                // Verify app exists if provided
                ThirdPartyApp? app = null;
                if (request.AppId.HasValue)
                {
                    app = await _context.ThirdPartyApps.FindAsync(request.AppId.Value);
                    if (app == null)
                        throw new ArgumentException("Third party app not found");
                }

                foreach (var userPoint in request.UserPoints)
                {
                    // Verify user exists
                    var user = await _context.Users.FindAsync(userPoint.UserId);
                    if (user == null)
                        continue; // Skip invalid users

                    // Calculate final points with multiplier
                    var finalPoints = userPoint.Points;
                    if (app != null)
                    {
                        finalPoints *= app.PointsMultiplier;
                    }

                    // Create transaction
                    var pointTransaction = new PointTransaction
                    {
                        UserId = userPoint.UserId,
                        AppId = request.AppId,
                        TransactionType = TransactionType.Earned,
                        Points = finalPoints,
                        Description = request.Description,
                        ReferenceId = request.ReferenceId,
                        Status = TransactionStatus.Completed,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.PointTransactions.Add(pointTransaction);

                    results.Add(new PointTransactionDto
                    {
                        Id = pointTransaction.Id,
                        UserId = pointTransaction.UserId,
                        UserName = user.FullName,
                        AppId = pointTransaction.AppId,
                        AppName = app?.AppName,
                        TransactionType = pointTransaction.TransactionType,
                        Points = pointTransaction.Points,
                        Description = pointTransaction.Description,
                        ReferenceId = pointTransaction.ReferenceId,
                        Status = pointTransaction.Status,
                        CreatedAt = pointTransaction.CreatedAt
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return results;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<UserWalletSummaryDto> GetUserWallet(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Wallet)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new ArgumentException("User not found");

            var recentTransactions = await _context.PointTransactions
                .Include(pt => pt.App)
                .Where(pt => pt.UserId == userId)
                .OrderByDescending(pt => pt.CreatedAt)
                .Take(10)
                .Select(pt => new PointTransactionDto
                {
                    Id = pt.Id,
                    UserId = pt.UserId,
                    UserName = user.FullName,
                    AppId = pt.AppId,
                    AppName = pt.App != null ? pt.App.AppName : null,
                    TransactionType = pt.TransactionType,
                    Points = pt.Points,
                    Description = pt.Description,
                    ReferenceId = pt.ReferenceId,
                    ExternalTransactionId = pt.ExternalTransactionId,
                    Status = pt.Status,
                    CreatedAt = pt.CreatedAt
                })
                .ToListAsync();

            return new UserWalletSummaryDto
            {
                UserId = userId,
                UserName = user.FullName,
                Balance = user.Wallet?.Balance ?? 0,
                TotalEarned = user.Wallet?.TotalEarned ?? 0,
                TotalRedeemed = user.Wallet?.TotalRedeemed ?? 0,
                RecentTransactions = recentTransactions
            };
        }

        public async Task<TransactionHistoryDto> GetTransactionHistory(int userId, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.PointTransactions
                .Include(pt => pt.User)
                .Include(pt => pt.App)
                .Where(pt => pt.UserId == userId)
                .OrderByDescending(pt => pt.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var transactions = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(pt => new PointTransactionDto
                {
                    Id = pt.Id,
                    UserId = pt.UserId,
                    UserName = pt.User.FirstName + " " + pt.User.LastName,
                    AppId = pt.AppId,
                    AppName = pt.App != null ? pt.App.AppName : null,
                    TransactionType = pt.TransactionType,
                    Points = pt.Points,
                    Description = pt.Description,
                    ReferenceId = pt.ReferenceId,
                    ExternalTransactionId = pt.ExternalTransactionId,
                    Status = pt.Status,
                    CreatedAt = pt.CreatedAt
                })
                .ToListAsync();

            return new TransactionHistoryDto
            {
                Transactions = transactions,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<TransactionHistoryDto> GetAllTransactions(int pageNumber = 1, int pageSize = 20)
        {
            var query = _context.PointTransactions
                .Include(pt => pt.User)
                .Include(pt => pt.App)
                .OrderByDescending(pt => pt.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var transactions = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(pt => new PointTransactionDto
                {
                    Id = pt.Id,
                    UserId = pt.UserId,
                    UserName = pt.User.FirstName + " " + pt.User.LastName,
                    AppId = pt.AppId,
                    AppName = pt.App != null ? pt.App.AppName : null,
                    TransactionType = pt.TransactionType,
                    Points = pt.Points,
                    Description = pt.Description,
                    ReferenceId = pt.ReferenceId,
                    ExternalTransactionId = pt.ExternalTransactionId,
                    Status = pt.Status,
                    CreatedAt = pt.CreatedAt
                })
                .ToListAsync();

            return new TransactionHistoryDto
            {
                Transactions = transactions,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }
    }

    public interface IPointsService
    {
        Task<PointTransactionDto> AddPoints(AddPointsRequestDto request);
        Task<List<PointTransactionDto>> AddBulkPoints(BulkPointsRequestDto request);
        Task<UserWalletSummaryDto> GetUserWallet(int userId);
        Task<TransactionHistoryDto> GetTransactionHistory(int userId, int pageNumber = 1, int pageSize = 10);
        Task<TransactionHistoryDto> GetAllTransactions(int pageNumber = 1, int pageSize = 20);
    }
}
