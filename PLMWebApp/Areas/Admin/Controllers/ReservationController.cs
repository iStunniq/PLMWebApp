using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        //private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        [BindProperty]
        public ReservationVM ReservationVM { get; set; }

        public ReservationController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public bool ValidateRole(string email, string role)
        {
            var user = _userManager.FindByEmailAsync(email).Result;
            return _userManager.IsInRoleAsync(user, role).Result;
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
                ReservationDetail = _unitOfWork.ReservationDetail.GetAll(u => u.OrderId == reservationId, includeProperties: "Batch,Batch.Product"),
                Carrier = _unitOfWork.ApplicationUser.GetAll().Where(u=>ValidateRole(u.Email,SD.Role_Courier)).Select(i => new SelectListItem
                {
                    Text = i.Email,
                    Value = i.Id.ToString()
                })
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
        public IActionResult ForProcessing()
        {
            var reservationHeaderFromDb = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, tracked: false);
            reservationHeaderFromDb.OrderStatus = SD.StatusInProcess;
            reservationHeaderFromDb.PaymentStatus = SD.PaymentStatusApproved;
            _unitOfWork.ReservationHeader.Update(reservationHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Reservation Status Updated Successfully";

            ReservationHeader reservationHeader = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, includeProperties: "ApplicationUser");
            _emailSender.SendEmailAsync(reservationHeader.ApplicationUser.Email, "Reservation is in Process! - Meatify", $"<p><h3>Your reservation is in process, {reservationHeader.ApplicationUser.FirstName}! " +
                $"This is for Reservation # {ReservationVM.ReservationHeader.Id}. </p> <p>Your reservation is now in the In Process tab, go to Reservations.</h3></p> <p><em>NOTICE: Cancelling reservations is handled by our Meatify Staff directly</em></p>" +
                $"<p><em>Please contact details for more information: #09219370070 - Gabriel</em></p>");

            IEnumerable<ApplicationUser> logEmployees = _unitOfWork.ApplicationUser.GetAll(u => ValidateRole(u.Email, SD.Role_Logistics));
            foreach(var man in logEmployees)
            {
                _emailSender.SendEmailAsync(man.Email, "Pst, new process", $"dude, there's an order for order number {ReservationVM.ReservationHeader.Id}");
            };

            return RedirectToAction("Details", "Reservation", new { reservationId = ReservationVM.ReservationHeader.Id });
        }
        
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Logistics + "," + SD.Role_Courier)]
        [ValidateAntiForgeryToken]
        public IActionResult ToCourier()
        {

            //_unitOfWork.ReservationHeader.UpdateStatus(ReservationVM.ReservationHeader.Id, SD.StatusInProcess);
            var reservationHeaderFromDb = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, tracked: false);
            reservationHeaderFromDb.OrderStatus = SD.StatusApproval;
            reservationHeaderFromDb.ShippingDate = ReservationVM.ReservationHeader.ShippingDate;
            reservationHeaderFromDb.Carrier = ReservationVM.ReservationHeader.Carrier;
            ApplicationUser carrier = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Carrier);
            _unitOfWork.ReservationHeader.Update(reservationHeaderFromDb);
            _unitOfWork.Save();

            ReservationHeader reservationHeader = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, includeProperties: "ApplicationUser");
            _emailSender.SendEmailAsync(reservationHeader.ApplicationUser.Email, "Reservation is for Approval! - Meatify", $"<p><h3>Your reservation is for approval, {reservationHeader.ApplicationUser.FirstName}! " +
                $"This is for Reservation # {ReservationVM.ReservationHeader.Id}. </p> <p>Your reservation is now in the Approval tab, go to Reservations.</h3></p> <p><em>NOTICE: Cancelling reservations is handled by our Meatify Staff directly</em></p>" +
                $"<p><em>Please contact details for more information: #09219370070 - Gabriel</em></p>");

            _emailSender.SendEmailAsync(carrier.Email, "Pst, new process", $"dude, there's an order for order number {ReservationVM.ReservationHeader.Id}");

            TempData["Success"] = "Reservation Status Updated Successfully";
            return RedirectToAction("Details", "Reservation", new { reservationId = ReservationVM.ReservationHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Logistics + "," + SD.Role_Courier)]
        [ValidateAntiForgeryToken]
        public IActionResult RejectOrder()
        {
            var reservationHeader = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, tracked: false);

            reservationHeader.OrderStatus = SD.StatusInProcess;
            reservationHeader.ShippingDate = ReservationVM.ReservationHeader.ShippingDate;
            reservationHeader.Carrier = "";

            _unitOfWork.ReservationHeader.Update(reservationHeader);

            _unitOfWork.Save();
            TempData["Success"] = "Reservation is for Shipping Status Updated Successfully";
            return RedirectToAction("Details", "Reservation", new { reservationId = ReservationVM.ReservationHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Logistics + "," + SD.Role_Courier)]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var reservationHeader = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, tracked: false);

            reservationHeader.OrderStatus = SD.StatusShipped;

            _unitOfWork.ReservationHeader.Update(reservationHeader);


            ReservationHeader reservationHeader2 = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, includeProperties: "ApplicationUser");
            _emailSender.SendEmailAsync(reservationHeader2.ApplicationUser.Email, "Reservation to be Shipped! - Meatify", $"<p><h3>Your reservation is to be shipped, {reservationHeader2.ApplicationUser.FirstName}! " +
                $"This is for Reservation # {ReservationVM.ReservationHeader.Id}. </p> <p>Your reservation is now in the ToShip tab, go to Reservations.</h3></p> <p><em>NOTICE: Cancelling reservations is handled by our Meatify Staff directly</em></p>" +
                $"<p><em>Please contact details for more information: #09219370070 - Gabriel</em></p>");

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
            reservationHeaderFromDb.ShippingDate = DateTime.Now;
            _unitOfWork.ReservationHeader.Update(reservationHeaderFromDb);

            ReservationHeader reservationHeader2 = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, includeProperties: "ApplicationUser");
            _emailSender.SendEmailAsync(reservationHeader2.ApplicationUser.Email, "Reservation Completed! - Meatify", $"<p><h3>Your reservation is completed, {reservationHeader2.ApplicationUser.FirstName}! " +
                $"This is for Reservation # {ReservationVM.ReservationHeader.Id}. </p> <p>Your reservation is now in the Completed tab, go to Reservations.</h3></p> <p><em>NOTICE: Regarding any concern/feedback, it is handled by our Meatify Staff directly</em></p>" +
                $"<p><em>Please contact details for more information: #09219370070 - Gabriel</em></p>");

            _unitOfWork.Save();
            TempData["Success"] = "Reservation Status Updated Successfully";
            return RedirectToAction("Details", "Reservation", new { reservationId = ReservationVM.ReservationHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Logistics)]
        [ValidateAntiForgeryToken]
        public IActionResult Pending()
        {

            //_unitOfWork.ReservationHeader.UpdateStatus(ReservationVM.ReservationHeader.Id, SD.StatusInProcess);
            var reservationHeaderFromDb = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, tracked: false);
            reservationHeaderFromDb.OrderStatus = SD.StatusPending;
            reservationHeaderFromDb.Carrier = "";
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
            IEnumerable<ReservationDetail> reservationDetail = _unitOfWork.ReservationDetail.GetAll(u => u.OrderId == ReservationVM.ReservationHeader.Id, includeProperties:"Batch,Batch.Product");
            reservationHeader.OrderStatus = SD.StatusCancelled;
            reservationHeader.PaymentStatus = SD.PaymentStatusRefunded;

            foreach (ReservationDetail detail in reservationDetail) {
                detail.Batch.Stock += detail.Count;
                _unitOfWork.Product.Update(detail.Batch.Product);
            };

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

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Sales) || User.IsInRole(SD.Role_Marketing) || User.IsInRole(SD.Role_Logistics) || User.IsInRole(SD.Role_Operation))
            {
                reservationHeaders = _unitOfWork.ReservationHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else if (User.IsInRole(SD.Role_Courier))
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                reservationHeaders = _unitOfWork.ReservationHeader.GetAll(u => u.ApplicationUserId == claim.Value || u.Carrier == claim.Value, includeProperties: "ApplicationUser");
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
                case "approval":
                    reservationHeaders = reservationHeaders.Where(u => u.OrderStatus == SD.StatusApproval);
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
