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
        }


        public ICategoryRepository Category { get; private set; }

        public IBatchRepository Batch { get; private set; }

        public IBrandRepository Brand { get; private set; }

        public IProductRepository Product { get; private set; }

        public IShoppingCartRepository ShoppingCart { get; private set; }

        public IApplicationUserRepository ApplicationUser { get; private set; }

        public IReservationHeaderRepository ReservationHeader { get; private set; }

        public IReservationDetailRepository ReservationDetail { get; private set; }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
