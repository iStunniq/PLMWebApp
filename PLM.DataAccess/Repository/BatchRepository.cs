using PLM.DataAccess.Repository.IRepository;
using PLM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.DataAccess.Repository
{
    public class BatchRepository : Repository<Batch>, IBatchRepository
    {
        private ApplicationDbContext _db;

        public BatchRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Batch obj)
        {
            var objFromDb = _db.Batches.FirstOrDefault(u => u.Id == obj.Id);
            if (objFromDb != null)
            {
                objFromDb.Expiry = obj.Expiry;
                objFromDb.BasePrice = obj.BasePrice;
                objFromDb.Stock = obj.Stock;
            }
        }
    }
}
