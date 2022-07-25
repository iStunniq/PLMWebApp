using PLM.DataAccess.Repository.IRepository;
using PLM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.DataAccess.Repository
{
    public class ReservationViewedRepository : Repository<ReservationViewed>, IReservationViewedRepository
    {
        private ApplicationDbContext _db;

        public ReservationViewedRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ReservationViewed obj)
        {
            _db.ReservationViews.Update(obj);
        }
    }
}
