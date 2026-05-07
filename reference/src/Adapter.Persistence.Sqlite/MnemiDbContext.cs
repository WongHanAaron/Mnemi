using Microsoft.EntityFrameworkCore;
using Mnemi.Domain.Entities;

namespace Mnemi.Adapter.Persistence.Sqlite;

/// <summary>
/// Entity Framework Core database context for Mnemi persistence.
/// Supports both file-based SQLite and in-memory SQLite for testing.
/// </summary>
public class MnemiDbContext : DbContext
{
    private readonly bool _useInMemory;
    private readonly string? _databasePath;

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<AuthConnectionEntity> AuthConnections => Set<AuthConnectionEntity>();
    public DbSet<DocumentSourceConfigEntity> DocumentSourceConfigs => Set<DocumentSourceConfigEntity>();

    /// <summary>
    /// Creates a file-based SQLite context.
    /// </summary>
    public MnemiDbContext(string databasePath)
    {
        _databasePath = databasePath ?? throw new ArgumentNullException(nameof(databasePath));
        _useInMemory = false;
    }

    /// <summary>
    /// Creates an in-memory SQLite context for testing.
    /// </summary>
    public MnemiDbContext(DbContextOptions<MnemiDbContext> options) : base(options)
    {
        _useInMemory = true;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            if (_useInMemory)
            {
                optionsBuilder.UseSqlite("DataSource=:memory:");
            }
            else
            {
                optionsBuilder.UseSqlite($"Data Source={_databasePath}");
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity configuration
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasIndex(e => e.Email).IsUnique();
        });

        // AuthConnection entity configuration
        modelBuilder.Entity<AuthConnectionEntity>(entity =>
        {
            entity.ToTable("AuthConnections");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Provider).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ProviderUserId).HasMaxLength(255).IsRequired();
            entity.Property(e => e.EncryptedAccessToken).IsRequired();
            entity.Property(e => e.EncryptedRefreshToken);
            entity.Property(e => e.ExpiresAt);
            entity.Property(e => e.Scopes).HasMaxLength(500).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.IsValid).IsRequired();

            entity.HasIndex(e => new { e.Provider, e.ProviderUserId }).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsValid);

            entity.HasOne<UserEntity>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DocumentSourceConfig entity configuration
        modelBuilder.Entity<DocumentSourceConfigEntity>(entity =>
        {
            entity.ToTable("DocumentSourceConfigs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.AuthConnectionId).IsRequired();
            entity.Property(e => e.Provider).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ProviderConfigJson).IsRequired();
            entity.Property(e => e.IsAccessible).IsRequired();
            entity.Property(e => e.LastErrorMessage);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.AuthConnectionId);
            entity.HasIndex(e => new { e.UserId, e.Provider });
            entity.HasIndex(e => e.IsAccessible);

            entity.HasOne<UserEntity>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
