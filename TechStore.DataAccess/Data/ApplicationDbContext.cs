using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechStore.Entities.Models;

namespace TechStore.DataAccess.Data
{
    // FIX 1: Use generic IdentityDbContext<ApplicationUser> so that custom
    // columns (Name, Address, City, OTPCode, OTPExpiry) are correctly mapped
    // to the AspNetUsers table. Using the non-generic base ignores our custom type.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Decimal precision for financial columns
            builder.Entity<Product>().Property(p => p.Price).HasColumnType("decimal(18,2)");
            builder.Entity<OrderHeader>().Property(o => o.TotalPrice).HasColumnType("decimal(18,2)");
            builder.Entity<OrderDetail>().Property(o => o.Price).HasColumnType("decimal(18,2)");

            // FIX 12: Add indexes on high-frequency foreign-key and filter columns.
            // Without explicit indexes SQL Server scans the entire table for each query.

            // Orders are frequently filtered/joined by user
            builder.Entity<OrderHeader>()
                .HasIndex(o => o.ApplicationUserId)
                .HasDatabaseName("IX_OrderHeaders_ApplicationUserId");

            // Cart items are always queried by user and by product
            builder.Entity<ShoppingCart>()
                .HasIndex(c => c.ApplicationUserId)
                .HasDatabaseName("IX_ShoppingCarts_ApplicationUserId");

            builder.Entity<ShoppingCart>()
                .HasIndex(c => c.ProductId)
                .HasDatabaseName("IX_ShoppingCarts_ProductId");

            // Products are regularly filtered by category
            builder.Entity<Product>()
                .HasIndex(p => p.CategoryId)
                .HasDatabaseName("IX_Products_CategoryId");

            // Category name should be unique to prevent duplicates
            builder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique()
                .HasDatabaseName("IX_Categories_Name_Unique");
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}
