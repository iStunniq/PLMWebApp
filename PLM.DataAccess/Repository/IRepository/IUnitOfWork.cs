using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }

        IBatchRepository Batch { get; }

        IBrandRepository Brand { get; }

        IProductRepository Product { get; }

        IShoppingCartRepository ShoppingCart { get; }

        IApplicationUserRepository ApplicationUser { get; }

        IReservationHeaderRepository ReservationHeader { get; }

        IReservationDetailRepository ReservationDetail { get; }

        IReservationViewedRepository ReservationViewed { get; }

        ISalesReportRepository SalesReport { get; }

        void Save();
    }
}
