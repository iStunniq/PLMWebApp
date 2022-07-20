//using PLM.DataAccess.Repository.IRepository;
//using PLM.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace PLM.DataAccess.Repository
//{
//    public class RegionRepository : Repository<Region>, IRegionRepository
//    {
//        private ApplicationDbContext _db;

//        public RegionRepository(ApplicationDbContext db) : base(db)
//        {
//            _db = db;
//        }

//        public void Update(Region obj)
//        {
//            _db.Regions.Update(obj);
//        }
//    }
//}
