using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.Models.ViewModels
{
    public class ReservationVM
    {
        public ReservationHeader ReservationHeader { get; set; }

        public IEnumerable<ReservationDetail> ReservationDetail { get; set; }
    }
}
