using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ThriftMedia.Infrastructure.Persistence.Models;

public partial class ThriftMediaDbContext : DbContext
{
    public ThriftMediaDbContext(DbContextOptions<ThriftMediaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<Medium> Media { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AppUsers__3214EC07696B0A76");

            entity.ToTable("AppUsers", "auth");

            entity.HasIndex(e => new { e.Provider, e.ProviderSub }, "IX_AppUsers_Provider_ProviderSub").IsUnique();

            entity.HasIndex(e => e.Role, "IX_AppUsers_SingleSiteAdmin")
                .IsUnique()
                .HasFilter("([Role]=(0))");

            entity.Property(e => e.CreatedAtUtc).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(320);
            entity.Property(e => e.LastSeenAtUtc).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Provider).HasMaxLength(200);
            entity.Property(e => e.ProviderSub).HasMaxLength(200);
        });

        modelBuilder.Entity<Medium>(entity =>
        {
            entity.HasKey(e => e.MediaId).HasName("PK_MediaItems");

            entity.ToTable("Media", "retail");

            entity.Property(e => e.Condition).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsTested).HasDefaultValue(false);
            entity.Property(e => e.MediaType).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ShelfLocation).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);

            entity.HasOne(d => d.Store).WithMany(p => p.Media)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StoreMedia_Store");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Store__3214EC075CC5639F");

            entity.ToTable("Store", "retail");

            entity.HasIndex(e => e.AppUserId, "UQ_Store_AppUserId").IsUnique();

            entity.Property(e => e.Address1).HasMaxLength(150);
            entity.Property(e => e.Address2).HasMaxLength(150);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.ExpirationDate).HasColumnType("datetime");
            entity.Property(e => e.IssueDate).HasColumnType("datetime");
            entity.Property(e => e.IssueingAuthority).HasMaxLength(100);
            entity.Property(e => e.LicenseNumber).HasMaxLength(100);
            entity.Property(e => e.LicenseStatus).HasMaxLength(20);
            entity.Property(e => e.LicenseType).HasMaxLength(50);
            entity.Property(e => e.OwerEmail).HasMaxLength(255);
            entity.Property(e => e.OwerPhoneNumber).HasMaxLength(50);
            entity.Property(e => e.OwnerFirstName).HasMaxLength(50);
            entity.Property(e => e.OwnerLastName).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Postcode).HasMaxLength(20);
            entity.Property(e => e.StoreName).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.WebsiteUrl).HasMaxLength(255);

            entity.HasOne(d => d.AppUser).WithOne(p => p.Store)
                .HasForeignKey<Store>(d => d.AppUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Store_AppUser");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
