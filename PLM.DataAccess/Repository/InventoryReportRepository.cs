using PLM.DataAccess.Repository.IRepository;
using PLM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.DataAccess.Repository
{
    public class InventoryReportRepository : Repository<InventoryReport>, IInventoryReportRepository
    {
        private ApplicationDbContext _db;

        public InventoryReportRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(InventoryReport obj)
        {
            _db.InventoryReports.Update(obj);
        }
    }
}
