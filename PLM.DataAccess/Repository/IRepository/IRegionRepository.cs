using PLM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.DataAccess.Repository.IRepository
{
    public interface IRegionRepository : IRepository<Region>
    {
        void Update(Region obj);
    }
}
