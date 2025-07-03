using BCrypt.Net;
using LoyaltyRewardsApi.Data;
using LoyaltyRewardsApi.DTOs;
using LoyaltyRewardsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyRewardsApi.Services
{
    public class UserService : IUserService
    {
        private readonly LoyaltyRewardsContext _context;

        public UserService(LoyaltyRewardsContext context)
        {
            _context = context;
        }

        public async Task<UserDto> Authenticate(string email, string password)
        {
            var user = await _context.Users.Include(u => u.Wallet)
                                           .SingleOrDefaultAsync(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Wallet = user.Wallet == null ? null : new UserWalletDto
                {
                    Id = user.Wallet.Id,
                    UserId = user.Wallet.UserId,
                    Balance = user.Wallet.Balance,
                    TotalEarned = user.Wallet.TotalEarned,
                    TotalRedeemed = user.Wallet.TotalRedeemed,
                    CreatedAt = user.Wallet.CreatedAt,
                    UpdatedAt = user.Wallet.UpdatedAt
                }
            };
        }

        public async Task<UserDto> Register(RegisterRequestDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new ArgumentException("Email is already registered.");
            }

            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new ArgumentException("Username is already taken.");
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.UserWallets.Add(new UserWallet
            {
                UserId = user.Id
            });

            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Wallet = new UserWalletDto
                {
                    UserId = user.Id
                }
            };
        }

        public async Task ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                throw new ArgumentException("Current password is incorrect.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<UserDto> UpdateUser(int userId, UpdateUserDto dto)
        {
            var user = await _context.Users.Include(u => u.Wallet)
                                           .SingleOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Username) && user.Username != dto.Username)
            {
                if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                {
                    throw new ArgumentException("Username is already taken.");
                }
                user.Username = dto.Username;
            }

            if (!string.IsNullOrWhiteSpace(dto.Email) && user.Email != dto.Email)
            {
                if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                {
                    throw new ArgumentException("Email is already registered.");
                }
                user.Email = dto.Email;
            }

            user.FirstName = dto.FirstName ?? user.FirstName;
            user.LastName = dto.LastName ?? user.LastName;
            user.Role = dto.Role ?? user.Role;
            user.IsActive = dto.IsActive ?? user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Wallet = user.Wallet == null ? null : new UserWalletDto
                {
                    Id = user.Wallet.Id,
                    UserId = user.Wallet.UserId,
                    Balance = user.Wallet.Balance,
                    TotalEarned = user.Wallet.TotalEarned,
                    TotalRedeemed = user.Wallet.TotalRedeemed,
                    CreatedAt = user.Wallet.CreatedAt,
                    UpdatedAt = user.Wallet.UpdatedAt
                }
            };
        }

        public async Task DeleteUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserListDto>> GetAllUsers()
        {
            return await _context.Users.Include(u => u.Wallet)
                                        .Select(user => new UserListDto
                                        {
                                            Id = user.Id,
                                            Username = user.Username,
                                            Email = user.Email,
                                            FullName = user.FirstName + " " + user.LastName,
                                            Role = user.Role,
                                            IsActive = user.IsActive,
                                            WalletBalance = user.Wallet != null ? user.Wallet.Balance : 0,
                                            CreatedAt = user.CreatedAt,
                                        })
                                        .ToListAsync();
        }

        public async Task<UserDto> GetUserById(int userId)
        {
            var user = await _context.Users.Include(u => u.Wallet)
                                           .SingleOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Wallet = user.Wallet == null ? null : new UserWalletDto
                {
                    Id = user.Wallet.Id,
                    UserId = user.Wallet.UserId,
                    Balance = user.Wallet.Balance,
                    TotalEarned = user.Wallet.TotalEarned,
                    TotalRedeemed = user.Wallet.TotalRedeemed,
                    CreatedAt = user.Wallet.CreatedAt,
                    UpdatedAt = user.Wallet.UpdatedAt
                }
            };
        }
    }

    public interface IUserService
    {
        Task<UserDto> Authenticate(string email, string password);
        Task<UserDto> Register(RegisterRequestDto request);
        Task ChangePassword(int userId, string currentPassword, string newPassword);
        Task<UserDto> UpdateUser(int userId, UpdateUserDto dto);
        Task DeleteUser(int userId);
        Task<List<UserListDto>> GetAllUsers();
        Task<UserDto> GetUserById(int userId);
    }
}
