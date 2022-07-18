using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLM.DataAccess.Repository.IRepository;
using PLM.Models;
using PLM.Models.ViewModels;
using PLM.Utility;
using System.Security.Claims;

namespace PLMWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ReservationController : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ReservationVM ReservationVM { get; set; }

        public ReservationController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int reservationId)
        {
            ReservationVM = new ReservationVM()
            {
                ReservationHeader = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == reservationId, includeProperties: "ApplicationUser"),
                ReservationDetail = _unitOfWork.ReservationDetail.GetAll(u => u.OrderId == reservationId, includeProperties: "Product"),
            };

            return View(ReservationVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Logistics)]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateReservationDetail()
        {
            var reservationHeaderFromDb = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, tracked: false);

            reservationHeaderFromDb.FirstName = ReservationVM.ReservationHeader.FirstName;
            reservationHeaderFromDb.LastName = ReservationVM.ReservationHeader.LastName;
            reservationHeaderFromDb.Phone = ReservationVM.ReservationHeader.Phone;
            reservationHeaderFromDb.Address = ReservationVM.ReservationHeader.Address;
            reservationHeaderFromDb.City = ReservationVM.ReservationHeader.City;
            reservationHeaderFromDb.ZipCode = ReservationVM.ReservationHeader.ZipCode;

            if (ReservationVM.ReservationHeader.Carrier != null)
            {
                reservationHeaderFromDb.Carrier = ReservationVM.ReservationHeader.Carrier;
            }

            _unitOfWork.ReservationHeader.Update(reservationHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Reservation Details Updated Successfully";
            return RedirectToAction("Details", "Reservation", new { reservationId = reservationHeaderFromDb.Id });
        }



        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Logistics + "," + SD.Role_Sales)]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveForProcessing()
        {
            var reservationHeaderFromDb = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, tracked: false);
            reservationHeaderFromDb.OrderStatus = SD.StatusApproved;
            reservationHeaderFromDb.PaymentStatus = SD.PaymentStatusApproved;
            _unitOfWork.ReservationHeader.Update(reservationHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Reservation Status Updated Successfully";
            return RedirectToAction("Details", "Reservation", new { reservationId = ReservationVM.ReservationHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Logistics)]
        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing()
        {

            //_unitOfWork.ReservationHeader.UpdateStatus(ReservationVM.ReservationHeader.Id, SD.StatusInProcess);
            var reservationHeaderFromDb = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, tracked: false);
            reservationHeaderFromDb.OrderStatus = SD.StatusInProcess;
            _unitOfWork.ReservationHeader.Update(reservationHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Reservation Status Updated Successfully";
            return RedirectToAction("Details", "Reservation", new { reservationId = ReservationVM.ReservationHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Logistics)]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var reservationHeader = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, tracked: false);

            reservationHeader.Carrier = ReservationVM.ReservationHeader.Carrier;
            reservationHeader.OrderStatus = SD.StatusShipped;
            reservationHeader.ShippingDate = DateTime.Now;

            _unitOfWork.ReservationHeader.Update(reservationHeader);

            _unitOfWork.Save();
            TempData["Success"] = "Reservation is for Shipping Status Updated Successfully";
            return RedirectToAction("Details", "Reservation", new { reservationId = ReservationVM.ReservationHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Logistics + "," + SD.Role_Courier)]
        [ValidateAntiForgeryToken]
        public IActionResult Completed()
        {

            //_unitOfWork.ReservationHeader.UpdateStatus(ReservationVM.ReservationHeader.Id, SD.StatusInProcess);
            var reservationHeaderFromDb = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, tracked: false);
            reservationHeaderFromDb.OrderStatus = SD.StatusCompleted;
            _unitOfWork.ReservationHeader.Update(reservationHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Reservation Status Updated Successfully";
            return RedirectToAction("Details", "Reservation", new { reservationId = ReservationVM.ReservationHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Logistics + "," + SD.Role_Sales + "," + SD.Role_Courier)]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder(IFormFile file)
        {
            var reservationHeader = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, tracked: false);

            reservationHeader.OrderStatus = SD.StatusCancelled;
            reservationHeader.PaymentStatus = SD.PaymentStatusRefunded;

            _unitOfWork.ReservationHeader.Update(reservationHeader);

            _unitOfWork.Save();
            TempData["Success"] = "Reservation Cancelled Successfully";
            return RedirectToAction("Details", "Reservation", new { reservationId = ReservationVM.ReservationHeader.Id });
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<ReservationHeader> reservationHeaders;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Sales) || User.IsInRole(SD.Role_Marketing) || User.IsInRole(SD.Role_Logistics) || User.IsInRole(SD.Role_Courier))
            {
                reservationHeaders = _unitOfWork.ReservationHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                reservationHeaders = _unitOfWork.ReservationHeader.GetAll(u => u.ApplicationUserId == claim.Value,includeProperties: "ApplicationUser");
            }

            

            switch(status)
            {
                case "pending":
                    reservationHeaders = reservationHeaders.Where(u => u.OrderStatus == SD.StatusPending);
                    break;
                case "approved":
                    reservationHeaders = reservationHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                case "inprocess":
                    reservationHeaders = reservationHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "shipped":
                    reservationHeaders = reservationHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "completed":
                    reservationHeaders = reservationHeaders.Where(u => u.OrderStatus == SD.StatusCompleted);
                    break;
                default:
                    break;
            }
            
            return Json(new { data = reservationHeaders });
        }
        #endregion
    }
}
