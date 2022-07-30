using PLM.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new CategoryRepository(_db);
            Batch = new BatchRepository(_db);
            Brand = new BrandRepository(_db);
            Product = new ProductRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            ShoppingCart = new ShoppingCartRepository(_db);
            ReservationHeader = new ReservationHeaderRepository(_db);
            ReservationDetail = new ReservationDetailRepository(_db);
            SalesReport = new SalesReportRepository(_db);
            DeliveryReport = new DeliveryReportRepository(_db);
            ReservationReport = new ReservationReportRepository(_db);
            ReportDetail = new ReportDetailRepository(_db);
            InventoryReport = new InventoryReportRepository(_db);
            InvReportDetail = new InvReportDetailRepository(_db);
            ReservationViewed = new ReservationViewedRepository(_db);
        }


        public ICategoryRepository Category { get; private set; }

        public IBatchRepository Batch { get; private set; }

        public IBrandRepository Brand { get; private set; }

        public IProductRepository Product { get; private set; }

        public IShoppingCartRepository ShoppingCart { get; private set; }

        public IApplicationUserRepository ApplicationUser { get; private set; }

        public IReservationHeaderRepository ReservationHeader { get; private set; }

        public IReservationDetailRepository ReservationDetail { get; private set; }

        public ISalesReportRepository SalesReport { get; private set; }

        public IDeliveryReportRepository DeliveryReport { get; private set; }

        public IReservationReportRepository ReservationReport { get; private set; }

        public IReportDetailRepository ReportDetail { get; private set; }

        public IInventoryReportRepository InventoryReport { get; private set; }

        public IInvReportDetailRepository InvReportDetail { get; private set; }

        public IReservationViewedRepository ReservationViewed { get; private set; }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
