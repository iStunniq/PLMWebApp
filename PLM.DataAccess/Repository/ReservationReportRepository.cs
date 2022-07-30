using PLM.DataAccess.Repository.IRepository;
using PLM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.DataAccess.Repository
{
    public class ReservationReportRepository : Repository<ReservationReport>, IReservationReportRepository
    {
        private ApplicationDbContext _db;

        public ReservationReportRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ReservationReport obj)
        {
            _db.ReservationReports.Update(obj);
        }
    }
}
