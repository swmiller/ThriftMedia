using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ThriftMedia.Data.Models;

public partial class ThriftMediaDbContext : DbContext
{
    public ThriftMediaDbContext() { }
    public ThriftMediaDbContext(DbContextOptions<ThriftMediaDbContext> options) : base(options) { }

    public virtual DbSet<Media> MediaItems { get; set; } = null!;
    public virtual DbSet<Store> Stores { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Media>(entity =>
        {
            entity.HasKey(e => e.MediaId);
            entity.ToTable("Media", "media");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.MediaType).HasMaxLength(50);
            entity.HasOne(d => d.Store).WithMany(p => p.MediaItems)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Media_Store");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Store", "media");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.StoreName).HasMaxLength(200);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.OwnsOne(e => e.Address, address =>
            {
                address.Property(a => a.Street).HasMaxLength(100);
                address.Property(a => a.City).HasMaxLength(50);
                address.Property(a => a.State).HasMaxLength(2);
                address.Property(a => a.ZipCode).HasMaxLength(10);
                address.Property(a => a.Country).HasMaxLength(50);
            });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}