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
                objFromDb.Stock = obj.Stock;
                if (obj.Stock == 0)
                {
                    objFromDb.StockStat = SD.StockZero;
                } else if (obj.Stock > 0 && obj.Stock < 10)
                {
                    objFromDb.StockStat = SD.StockLow;
                } else if (obj.Stock >= 10 && obj.Stock < 20)
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
