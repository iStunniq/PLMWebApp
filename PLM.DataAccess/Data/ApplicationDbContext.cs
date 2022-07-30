using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PLM.Models;


namespace PLM.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Batch> Batches { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }

        public DbSet<SalesReport> SalesReports { get; set; }

        public DbSet<DeliveryReport> DeliveryReports { get; set; }

        public DbSet<InventoryReport> InventoryReports { get; set; }
        public DbSet<InvReportDetail> InvReportDetails { get; set; }

        public DbSet<ReservationReport> ReservationReports { get; set; }
        public DbSet<ReportDetail> ReportDetails { get; set; }
        public DbSet<ReservationHeader> ReservationHeaders { get; set; }



        public DbSet<ReservationDetail> ReservationDetails { get; set; }
        
        public DbSet<ReservationViewed> ReservationViews { get; set; }
    }
}
