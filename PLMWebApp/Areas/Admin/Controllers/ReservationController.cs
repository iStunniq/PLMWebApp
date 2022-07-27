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

        public void AlertLogistics(ReservationHeader reservationHeader)
        {
            IEnumerable<ApplicationUser> logEmployees = _unitOfWork.ApplicationUser.GetAll().Where(u => ValidateRole(u.Email, SD.Role_Logistics));

            foreach (var man in logEmployees)
            {
                ReservationViewed view = new();
                view.OrderId = reservationHeader.Id;
                view.AlertEmail = man.Email;
                _unitOfWork.ReservationViewed.Add(view);
                _unitOfWork.Save();
            };
        }

        public void AlertAdmin(ReservationHeader reservationHeader)
        {
            IEnumerable<ApplicationUser> Admins = _unitOfWork.ApplicationUser.GetAll().Where(u => ValidateRole(u.Email, SD.Role_Admin));

            foreach (var man in Admins)
            {
                ReservationViewed view = new();
                view.OrderId = reservationHeader.Id;
                view.AlertEmail = man.Email;
                _unitOfWork.ReservationViewed.Add(view);
                _unitOfWork.Save();
            };
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
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ApplicationUser user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            ReservationViewed view = _unitOfWork.ReservationViewed.GetFirstOrDefault(u => u.OrderId == reservationId && u.AlertEmail == user.Email);
            if (view != null) { 
                _unitOfWork.ReservationViewed.Remove(view);
                _unitOfWork.Save();
            }
            return View(ReservationVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Logistics)]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateReservationDetail()
        {
            var reservationHeaderFromDb = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, includeProperties: "ApplicationUser", tracked: false);

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
            var reservationHeaderFromDb = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, includeProperties: "ApplicationUser", tracked: false);
            reservationHeaderFromDb.OrderStatus = SD.StatusInProcess;
            reservationHeaderFromDb.PaymentStatus = SD.PaymentStatusApproved;
            if (reservationHeaderFromDb.COD)
            {
                reservationHeaderFromDb.PaymentStatus = SD.PaymentStatusDelayedPayment;
            }
            _unitOfWork.ReservationHeader.Update(reservationHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Reservation Status Updated Successfully";
            ReservationHeader reservationHeader = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, includeProperties: "ApplicationUser");
            _emailSender.SendEmailAsync(reservationHeader.ApplicationUser.Email, "Reservation is in Process! - Meatify", $"<p><h3>Your reservation is in process, {reservationHeader.ApplicationUser.FirstName}! " +
                $"This is for Reservation # {ReservationVM.ReservationHeader.Id}. </p> <p>Your reservation is now in the In Process tab, go to Reservations.</h3></p> <p><em>NOTICE: Cancelling reservations is handled by our Meatify Staff directly</em></p>" +
                $"<p><em>Please contact details for more information: #09219370070 - Gabriel</em></p>");

            _unitOfWork.ReservationViewed.RemoveRange(_unitOfWork.ReservationViewed.GetAll(u => u.OrderId == reservationHeader.Id));
            ReservationViewed view = new();
            view.OrderId = reservationHeader.Id;
            view.AlertEmail = reservationHeader.ApplicationUser.Email;
            _unitOfWork.ReservationViewed.Add(view);
            _unitOfWork.Save();

            IEnumerable<ApplicationUser> logEmployees = _unitOfWork.ApplicationUser.GetAll().Where(u => ValidateRole(u.Email, SD.Role_Logistics));

            foreach (var man in logEmployees)
            {
                _emailSender.SendEmailAsync(man.Email, "Reservation For Processing - Meatify", $"<p><h3>{man.FirstName}, please take action for reservation number {ReservationVM.ReservationHeader.Id}. Go to Reservations.</h3></p>");
            };

            AlertLogistics(reservationHeader);
            
            return RedirectToAction("Details", "Reservation", new { reservationId = ReservationVM.ReservationHeader.Id });
        }
        
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Logistics + "," + SD.Role_Courier)]
        [ValidateAntiForgeryToken]
        public IActionResult ToCourier()
        {

            //_unitOfWork.ReservationHeader.UpdateStatus(ReservationVM.ReservationHeader.Id, SD.StatusInProcess);
            var reservationHeaderFromDb = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, includeProperties: "ApplicationUser", tracked: false);
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

            _emailSender.SendEmailAsync(carrier.Email, "Reservation for Courier Approval - Meatify", $"<p><h3>{carrier.FirstName}, please take action for reservation number {ReservationVM.ReservationHeader.Id}. Go to Reservations.</h3></p>");

            _unitOfWork.ReservationViewed.RemoveRange(_unitOfWork.ReservationViewed.GetAll(u => u.OrderId == reservationHeader.Id));
            ReservationViewed view = new();
            view.OrderId = reservationHeader.Id;
            view.AlertEmail = reservationHeader.ApplicationUser.Email;
            _unitOfWork.ReservationViewed.Add(view);
            _unitOfWork.Save();

            ReservationViewed view2 = new();
            view2.OrderId = reservationHeader.Id;
            view2.AlertEmail = carrier.Email;
            _unitOfWork.ReservationViewed.Add(view2);
            _unitOfWork.Save();

            TempData["Success"] = "Reservation is for Courier Approval; Status Updated Successfully";
            return RedirectToAction("Details", "Reservation", new { reservationId = ReservationVM.ReservationHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Logistics + "," + SD.Role_Courier)]
        [ValidateAntiForgeryToken]
        public IActionResult RejectOrder()
        {
            var reservationHeader = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, includeProperties:("ApplicationUser"),tracked: false);

            reservationHeader.OrderStatus = SD.StatusInProcess;
            reservationHeader.ShippingDate = ReservationVM.ReservationHeader.ShippingDate;
            reservationHeader.Carrier = "";

            _unitOfWork.ReservationHeader.Update(reservationHeader);

            _unitOfWork.Save();

            _unitOfWork.ReservationViewed.RemoveRange(_unitOfWork.ReservationViewed.GetAll(u => u.OrderId == reservationHeader.Id));
            ReservationViewed view = new();
            view.OrderId = reservationHeader.Id;
            view.AlertEmail = reservationHeader.ApplicationUser.Email;
            _unitOfWork.ReservationViewed.Add(view);
            _unitOfWork.Save();

            ReservationHeader reservationHeader2 = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, includeProperties: "ApplicationUser");
            _emailSender.SendEmailAsync(reservationHeader2.ApplicationUser.Email, "Reservation is Rejected - Meatify", $"<p><h3>Your reservation is rejected, {reservationHeader2.ApplicationUser.FirstName}. " +
                $"This is for Reservation # {ReservationVM.ReservationHeader.Id}. </p> <p>Your reservation is now in the All tab, go to Reservations.</h3></p>" +
                $"<p><em>Please contact details for more information: #09219370070 - Gabriel</em></p>");

            IEnumerable<ApplicationUser> logEmployees = _unitOfWork.ApplicationUser.GetAll().Where(u => ValidateRole(u.Email, SD.Role_Logistics));

            foreach (var man in logEmployees)
            {
                _emailSender.SendEmailAsync(man.Email, "Reservation is Rejected - Meatify", $"<p><h3>{man.FirstName}, please check action for reservation number {ReservationVM.ReservationHeader.Id}. Go to Reservations.</h3></p>");
            };

            AlertLogistics(reservationHeader);

            IEnumerable<ApplicationUser> admins = _unitOfWork.ApplicationUser.GetAll().Where(u => ValidateRole(u.Email, SD.Role_Admin));
            foreach (var man in admins)
            {
                _emailSender.SendEmailAsync(man.Email, "Reservation is Rejected - Meatify", $"<p><h3>{man.FirstName}, please check action for reservation number {ReservationVM.ReservationHeader.Id}. Go to Reservations.</h3></p>");
            };

            AlertAdmin(reservationHeader);

            TempData["Success"] = "Reservation is Rejected; Status Updated Successfully";
            return RedirectToAction("Details", "Reservation", new { reservationId = ReservationVM.ReservationHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Logistics + "," + SD.Role_Courier)]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var reservationHeader = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, includeProperties: "ApplicationUser", tracked: false);

            reservationHeader.OrderStatus = SD.StatusShipped;

            _unitOfWork.ReservationHeader.Update(reservationHeader);

            _unitOfWork.ReservationViewed.RemoveRange(_unitOfWork.ReservationViewed.GetAll(u => u.OrderId == reservationHeader.Id));
            ReservationViewed view = new();
            view.OrderId = reservationHeader.Id;
            view.AlertEmail = reservationHeader.ApplicationUser.Email;
            _unitOfWork.ReservationViewed.Add(view);
            _unitOfWork.Save();

            ReservationHeader reservationHeader2 = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, includeProperties: "ApplicationUser");
            _emailSender.SendEmailAsync(reservationHeader2.ApplicationUser.Email, "Reservation to be Shipped! - Meatify", $"<p><h3>Your reservation is to be shipped, {reservationHeader2.ApplicationUser.FirstName}! " +
                $"This is for Reservation # {ReservationVM.ReservationHeader.Id}. </p> <p>Your reservation is now in the ToShip tab, go to Reservations.</h3></p> <p><em>NOTICE: Cancelling reservations is handled by our Meatify Staff directly</em></p>" +
                $"<p><em>Please contact details for more information: #09219370070 - Gabriel</em></p>");

            IEnumerable<ApplicationUser> admins = _unitOfWork.ApplicationUser.GetAll().Where(u => ValidateRole(u.Email, SD.Role_Admin));
            foreach (var man in admins)
            {
                _emailSender.SendEmailAsync(man.Email, "Reservation to be Shipped - Meatify", $"<p><h3>{man.FirstName}, please check action for reservation number {ReservationVM.ReservationHeader.Id}. Go to Reservations.</h3></p>");
            };

            AlertAdmin(reservationHeader);

            IEnumerable<ApplicationUser> logEmployees = _unitOfWork.ApplicationUser.GetAll().Where(u => ValidateRole(u.Email, SD.Role_Logistics));

            foreach (var man in logEmployees)
            {
                _emailSender.SendEmailAsync(man.Email, "Reservation to be Shipped - Meatify", $"<p><h3>{man.FirstName}, please check action for reservation number {ReservationVM.ReservationHeader.Id}. Go to Reservations.</h3></p>");
            };

            AlertLogistics(reservationHeader);

            _unitOfWork.Save();
            TempData["Success"] = "Reservation is for Shipping; Status Updated Successfully";
            return RedirectToAction("Details", "Reservation", new { reservationId = ReservationVM.ReservationHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Logistics + "," + SD.Role_Courier)]
        [ValidateAntiForgeryToken]
        public IActionResult Completed()
        {

            //_unitOfWork.ReservationHeader.UpdateStatus(ReservationVM.ReservationHeader.Id, SD.StatusInProcess);
            var reservationHeaderFromDb = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, includeProperties: "ApplicationUser", tracked: false);
            reservationHeaderFromDb.OrderStatus = SD.StatusCompleted;
            reservationHeaderFromDb.ShippingDate = DateTime.Now;
            if (reservationHeaderFromDb.COD)
            {
                reservationHeaderFromDb.PaymentDate = DateTime.Now;
                reservationHeaderFromDb.PaymentStatus = SD.PaymentStatusApproved;
            }
            _unitOfWork.ReservationHeader.Update(reservationHeaderFromDb);
            _unitOfWork.ReservationViewed.RemoveRange(_unitOfWork.ReservationViewed.GetAll(u => u.OrderId == reservationHeaderFromDb.Id));
            ReservationViewed view = new();
            view.OrderId = reservationHeaderFromDb.Id;
            view.AlertEmail = reservationHeaderFromDb.ApplicationUser.Email;
            _unitOfWork.ReservationViewed.Add(view);
            _unitOfWork.Save();

            ReservationHeader reservationHeader2 = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, includeProperties: "ApplicationUser");
            _emailSender.SendEmailAsync(reservationHeader2.ApplicationUser.Email, "Reservation Completed! - Meatify", $"<p><h3>Your reservation is completed, {reservationHeader2.ApplicationUser.FirstName}! " +
                $"This is for Reservation # {ReservationVM.ReservationHeader.Id}. </p> <p>Your reservation is now in the Completed tab, go to Reservations.</h3></p> <p><em>NOTICE: Regarding any concern/feedback, it is handled by our Meatify Staff directly</em></p>" +
                $"<p><em>Please contact details for more information: #09219370070 - Gabriel</em></p>");


            IEnumerable<ApplicationUser> admins = _unitOfWork.ApplicationUser.GetAll().Where(u => ValidateRole(u.Email, SD.Role_Admin));
            foreach (var man in admins)
            {
                _emailSender.SendEmailAsync(man.Email, "Reservation is Completed - Meatify", $"<p><h3>{man.FirstName}, please check action for reservation number {ReservationVM.ReservationHeader.Id}. Go to Reservations.</h3></p>");
            };

            AlertAdmin(reservationHeaderFromDb);

            _unitOfWork.Save();
            TempData["Success"] = "Reservation is Completed; Status Updated Successfully";
            return RedirectToAction("Details", "Reservation", new { reservationId = ReservationVM.ReservationHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Logistics)]
        [ValidateAntiForgeryToken]
        public IActionResult Pending()
        {

            //_unitOfWork.ReservationHeader.UpdateStatus(ReservationVM.ReservationHeader.Id, SD.StatusInProcess);
            var reservationHeaderFromDb = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, includeProperties: "ApplicationUser", tracked: false);
            reservationHeaderFromDb.OrderStatus = SD.StatusPending;
            reservationHeaderFromDb.Carrier = "";
            _unitOfWork.ReservationHeader.Update(reservationHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Reservation is Pending; Status Updated Successfully";
            return RedirectToAction("Details", "Reservation", new { reservationId = ReservationVM.ReservationHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Logistics + "," + SD.Role_Sales + "," + SD.Role_Courier)]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder(IFormFile file)
        {
            var reservationHeader = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id,includeProperties:"ApplicationUser", tracked: false);
            IEnumerable<ReservationDetail> reservationDetail = _unitOfWork.ReservationDetail.GetAll(u => u.OrderId == ReservationVM.ReservationHeader.Id, includeProperties: "Batch,Batch.Product");
            reservationHeader.OrderStatus = SD.StatusCancelled;
            reservationHeader.PaymentStatus = SD.PaymentStatusRefunded;
            if (reservationHeader.COD) { 
                reservationHeader.PaymentStatus = SD.PaymentStatusRejected;
            };
            reservationHeader.CancelReason = ReservationVM.ReservationHeader.CancelReason;

            foreach (ReservationDetail detail in reservationDetail) {
                detail.Batch.Stock += detail.Count;
                _unitOfWork.Product.Update(detail.Batch.Product);
            };

            _unitOfWork.ReservationHeader.Update(reservationHeader);

            _unitOfWork.Save();
            TempData["Success"] = "Reservation is Cancelled; Status Updated Successfully";

            _unitOfWork.ReservationViewed.RemoveRange(_unitOfWork.ReservationViewed.GetAll(u => u.OrderId == reservationHeader.Id));
            ReservationViewed view = new();
            view.OrderId = reservationHeader.Id;
            view.AlertEmail = reservationHeader.ApplicationUser.Email;
            _unitOfWork.ReservationViewed.Add(view);
            _unitOfWork.Save();

            ReservationHeader reservationHeader2 = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == ReservationVM.ReservationHeader.Id, includeProperties: "ApplicationUser");
            _emailSender.SendEmailAsync(reservationHeader2.ApplicationUser.Email, "Reservation Cancelled - Meatify", $"<p><h3>Your reservation was cancelled, {reservationHeader2.ApplicationUser.FirstName}. " +
                $"This is for Reservation # {ReservationVM.ReservationHeader.Id}. </p> <p>Your reservation was cancelled with the reason of {reservationHeader2.CancelReason}, check your reservations.</p> <p><em>NOTICE: Regarding any concern/feedback, it is handled by our Meatify Staff directly</em></p>" +
                $"<p><em>Please contact details for more information: #09219370070 - Gabriel</em></p>");

            IEnumerable<ApplicationUser> admins = _unitOfWork.ApplicationUser.GetAll().Where(u => ValidateRole(u.Email, SD.Role_Admin));
            foreach (var man in admins)
            {
                _emailSender.SendEmailAsync(man.Email, "Reservation Cancelled - Meatify", $"<p><h3>{man.FirstName}, please check action for reservation number {ReservationVM.ReservationHeader.Id}. Go to Reservations.</h3></p> <p>This reservation was cancelled with the reason of {reservationHeader2.CancelReason}.</p>");
            };

            IEnumerable<ApplicationUser> logEmployees = _unitOfWork.ApplicationUser.GetAll().Where(u => ValidateRole(u.Email, SD.Role_Logistics));

            foreach (var man in logEmployees)
            {
                _emailSender.SendEmailAsync(man.Email, "Reservation Cancelled - Meatify", $"<p><h3>{man.FirstName}, please check action for reservation number {ReservationVM.ReservationHeader.Id}. Go to Reservations.</h3></p> <p>This reservation was cancelled with the reason of {reservationHeader2.CancelReason}.</p>");
            };

            IEnumerable<ApplicationUser> salEmployees = _unitOfWork.ApplicationUser.GetAll().Where(u => ValidateRole(u.Email, SD.Role_Sales));

            foreach (var man in salEmployees)
            {
                _emailSender.SendEmailAsync(man.Email, "Reservation Cancelled - Meatify", $"<p><h3>{man.FirstName}, please check action for reservation number {ReservationVM.ReservationHeader.Id}. Go to Reservations.</h3></p> <p>This reservation was cancelled with the reason of {reservationHeader2.CancelReason}.</p>");
            };

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

            foreach (var head in reservationHeaders)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                ApplicationUser user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
                ReservationViewed view = _unitOfWork.ReservationViewed.GetFirstOrDefault(u => u.OrderId == head.Id && u.AlertEmail == user.Email);
                if (view != null)
                {
                    head.Viewed = false;
                }
                else
                {
                    head.Viewed = true;
                }
            };

            return Json(new { data = reservationHeaders });
        }
        #endregion
    }
}
