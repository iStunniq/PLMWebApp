using PLM.DataAccess.Repository.IRepository;
using PLM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.DataAccess.Repository
{
    public class ReservationDetailRepository : Repository<ReservationDetail>, IReservationDetailRepository
    {
        private ApplicationDbContext _db;

        public ReservationDetailRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ReservationDetail obj)
        {
            _db.ReservationDetails.Update(obj);
        }
    }
}
