using PLM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.DataAccess.Repository.IRepository
{
    public interface IReportDetailRepository : IRepository<ReportDetail>
    {
        void Update(ReportDetail obj);
    }
}
