using PLM.DataAccess.Repository.IRepository;
using PLM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.DataAccess.Repository
{
    public class DeliveryReportRepository : Repository<DeliveryReport>, IDeliveryReportRepository
    {
        private ApplicationDbContext _db;

        public DeliveryReportRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(DeliveryReport obj)
        {
            _db.DeliveryReports.Update(obj);
        }
    }
}
