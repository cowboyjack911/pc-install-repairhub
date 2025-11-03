using CrackedScreenCare.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CrackedScreenCare.Infrastructure.Data;

/// <summary>
/// Main application DbContext with PostgreSQL support and ASP.NET Core Identity.
/// Uses PostgreSQL schemas for modular separation (ticketing, inventory, etc.).
/// Configured with Npgsql provider for sequential UUIDv7 generation.
/// </summary>
public class AppDbContext : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Core entities
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<RepairTicket> RepairTickets => Set<RepairTicket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure PostgreSQL schemas for modular separation
        // Identity tables in default schema
        modelBuilder.Entity<IdentityUser<Guid>>().ToTable("AspNetUsers", schema: "identity");
        modelBuilder.Entity<IdentityRole<Guid>>().ToTable("AspNetRoles", schema: "identity");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("AspNetUserRoles", schema: "identity");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("AspNetUserClaims", schema: "identity");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("AspNetUserLogins", schema: "identity");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("AspNetRoleClaims", schema: "identity");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("AspNetUserTokens", schema: "identity");

        // Core business entities in ticketing schema (asset-centric model)
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers", schema: "ticketing");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(500);
            
            // Configure UUIDv7 for sequential IDs (index-friendly)
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.ToTable("assets", schema: "ticketing");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DeviceType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Manufacturer).HasMaxLength(100);
            entity.Property(e => e.Model).HasMaxLength(200);
            entity.Property(e => e.SerialNumber).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            
            // One-to-many relationship: Customer -> Assets
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Assets)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RepairTicket>(entity =>
        {
            entity.ToTable("repair_tickets", schema: "ticketing");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.EstimatedCost).HasPrecision(10, 2);
            entity.Property(e => e.ActualCost).HasPrecision(10, 2);
            entity.Property(e => e.TechnicianNotes).HasMaxLength(2000);
            
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            
            // One-to-many relationship: Asset -> RepairTickets
            // This is the core of the asset-centric model
            entity.HasOne(e => e.Asset)
                .WithMany(a => a.RepairTickets)
                .HasForeignKey(e => e.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
