using PLM.DataAccess.Repository.IRepository;
using PLM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.DataAccess.Repository
{
    public class InvReportDetailRepository : Repository<InvReportDetail>, IInvReportDetailRepository
    {
        private ApplicationDbContext _db;

        public InvReportDetailRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(InvReportDetail obj)
        {
            _db.InvReportDetails.Update(obj);
        }
    }
}
