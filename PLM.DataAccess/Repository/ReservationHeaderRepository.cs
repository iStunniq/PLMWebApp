using PLM.DataAccess.Repository.IRepository;
using PLM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.DataAccess.Repository
{
    public class ReservationHeaderRepository : Repository<ReservationHeader>, IReservationHeaderRepository
    {
        private ApplicationDbContext _db;

        public ReservationHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ReservationHeader obj)
        {
            _db.ReservationHeaders.Update(obj);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var reservationFromDb = _db.ReservationHeaders.FirstOrDefault(u => u.Id == id);
            if(reservationFromDb != null)
            {
                reservationFromDb.PaymentStatus = paymentStatus;
            }
        }
    }
}
