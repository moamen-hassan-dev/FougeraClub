using FougeraClub1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FougeraClub1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
              
        }

        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PurchaseOrder> purchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> purchaseOrderItems { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PurchaseOrderItem>()
                .Property(p => p.UnitPrice)
                .HasPrecision(18, 2);
        }

    }
}
