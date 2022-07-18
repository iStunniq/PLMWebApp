using PLM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.DataAccess.Repository.IRepository
{
    public interface IReservationHeaderRepository : IRepository<ReservationHeader>
    {
        void Update(ReservationHeader obj);

        void UpdateStatus(int id, string orderStatus, string? paymentStatus=null);

    }
}
