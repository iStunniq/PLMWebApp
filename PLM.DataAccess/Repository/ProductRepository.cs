using PLM.DataAccess.Repository.IRepository;
using PLM.Models;
using PLM.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product obj)
        {
            var objFromDb = _db.Products.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = obj.Name;
                objFromDb.Description = obj.Description;
                objFromDb.Price = obj.Price;
                objFromDb.CategoryId = obj.CategoryId;
                objFromDb.BrandId = obj.BrandId;
                var firstbatch = _db.Batches.Where(u => u.ProductId == obj.Id && u.Stock>0).FirstOrDefault();
                if (firstbatch == null)
                {
                    objFromDb.Expiry = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified);
                }
                else 
                { 
                    objFromDb.Expiry = firstbatch.Expiry;
                }
                IEnumerable<Batch> batches = _db.Batches.Where(u => u.ProductId == obj.Id).ToList();
                obj.Stock = 0;
                foreach (Batch batch in batches)
                {
                    obj.Stock = obj.Stock + batch.Stock;
                };
                objFromDb.Stock = obj.Stock;
                if (obj.Stock == 0)
                {
                    objFromDb.StockStat = SD.StockZero;
                } else if (obj.Stock > 0 && obj.Stock < SD.Mid)
                {
                    objFromDb.StockStat = SD.StockLow;
                } else if (obj.Stock >= SD.Mid && obj.Stock < SD.High)
                {
                    objFromDb.StockStat = SD.StockMid;
                } else {
                    objFromDb.StockStat = SD.StockHigh;
                };

                if (obj.ImageUrl != null)
                {
                    objFromDb.ImageUrl = obj.ImageUrl;
                }
            }
        }
    }
}
