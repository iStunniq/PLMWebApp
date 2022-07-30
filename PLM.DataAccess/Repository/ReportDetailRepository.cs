using PLM.DataAccess.Repository.IRepository;
using PLM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.DataAccess.Repository
{
    public class ReportDetailRepository : Repository<ReportDetail>, IReportDetailRepository
    {
        private ApplicationDbContext _db;

        public ReportDetailRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ReportDetail obj)
        {
            _db.ReportDetails.Update(obj);
        }
    }
}
