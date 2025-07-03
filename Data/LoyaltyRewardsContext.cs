using LoyaltyRewardsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyaltyRewardsApi.Data
{
    public class LoyaltyRewardsContext : DbContext
    {
        public LoyaltyRewardsContext(DbContextOptions<LoyaltyRewardsContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserWallet> UserWallets { get; set; } = null!;
        public DbSet<ThirdPartyApp> ThirdPartyApps { get; set; } = null!;
        public DbSet<PointTransaction> PointTransactions { get; set; } = null!;
        public DbSet<RewardsCatalog> RewardsCatalog { get; set; } = null!;
        public DbSet<Redemption> Redemptions { get; set; } = null!;
        public DbSet<UserSession> UserSessions { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Role);
                
                entity.Property(e => e.Role)
                    .HasConversion<string>();
            });

            // UserWallet entity configuration
            modelBuilder.Entity<UserWallet>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId).IsUnique();
                
                entity.HasOne(e => e.User)
                    .WithOne(u => u.Wallet!)
                    .HasForeignKey<UserWallet>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ThirdPartyApp entity configuration
            modelBuilder.Entity<ThirdPartyApp>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ApiKey).IsUnique();
                entity.HasIndex(e => e.AppName);
            });

            // PointTransaction entity configuration
            modelBuilder.Entity<PointTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.TransactionType);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.ReferenceId);
                entity.HasIndex(e => new { e.UserId, e.CreatedAt });

                entity.Property(e => e.TransactionType)
                    .HasConversion<string>();
                
                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Transactions)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.App)
                    .WithMany(a => a.Transactions!)
                    .HasForeignKey(e => e.AppId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // RewardsCatalog entity configuration
            modelBuilder.Entity<RewardsCatalog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.PointsRequired);
                entity.HasIndex(e => e.IsActive);
            });

            // Redemption entity configuration
            modelBuilder.Entity<Redemption>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.RedemptionCode).IsUnique();
                entity.HasIndex(e => new { e.UserId, e.CreatedAt });

                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Redemptions)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Reward)
                    .WithMany(r => r.Redemptions)
                    .HasForeignKey(e => e.RewardId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ProcessedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.ProcessedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // UserSession entity configuration
            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.TokenHash);
                entity.HasIndex(e => e.ExpiresAt);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Sessions)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // AuditLog entity configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => new { e.EntityType, e.EntityId });
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure decimal precision - these will be handled at database level
            // Entity Framework Core for MySQL will use default decimal configuration
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Update timestamps
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is User user && entry.State == EntityState.Modified)
                {
                    user.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is UserWallet wallet && entry.State == EntityState.Modified)
                {
                    wallet.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is ThirdPartyApp app && entry.State == EntityState.Modified)
                {
                    app.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is RewardsCatalog reward && entry.State == EntityState.Modified)
                {
                    reward.UpdatedAt = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
