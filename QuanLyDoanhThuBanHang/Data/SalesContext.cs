using Microsoft.EntityFrameworkCore;
using SalesManagementApp.Models;

namespace SalesManagementApp.Data
{
    public class SalesContext : DbContext
    {
        public SalesContext(DbContextOptions<SalesContext> options) : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>()
                .Property(t => t.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Discount)
                .HasColumnType("decimal(18,2)");
        }
    }
}
