using PLM.DataAccess.Repository.IRepository;
using PLM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.DataAccess.Repository
{
    public class SalesReportRepository : Repository<SalesReport>, ISalesReportRepository
    {
        private ApplicationDbContext _db;

        public SalesReportRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(SalesReport obj)
        {
            _db.SalesReports.Update(obj);
        }
    }
}
