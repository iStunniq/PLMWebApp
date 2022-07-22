using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLM.DataAccess.Repository.IRepository;
using PLM.Models;
using PLM.Models.ViewModels;
using PLM.Utility;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace PLMWebApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IEmailSender _emailSender;


        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        
        public int OrderTotal { get; set; }
        
        public CartController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;

            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, 
                includeProperties: "Product"),
                ReservationHeader = new()
            };
            
            foreach(var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price);
                ShoppingCartVM.ReservationHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;

            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Product"),
                ReservationHeader = new()
            };

            ShoppingCartVM.ReservationHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            ShoppingCartVM.ReservationHeader.ApplicationUserId = ShoppingCartVM.ReservationHeader.ApplicationUser.Id;
            ShoppingCartVM.ReservationHeader.FirstName = ShoppingCartVM.ReservationHeader.ApplicationUser.FirstName;
            ShoppingCartVM.ReservationHeader.LastName = ShoppingCartVM.ReservationHeader.ApplicationUser.LastName;
            ShoppingCartVM.ReservationHeader.Phone = ShoppingCartVM.ReservationHeader.ApplicationUser.Phone;
            ShoppingCartVM.ReservationHeader.Address = ShoppingCartVM.ReservationHeader.ApplicationUser.Address;
            ShoppingCartVM.ReservationHeader.City = ShoppingCartVM.ReservationHeader.ApplicationUser.City;
            ShoppingCartVM.ReservationHeader.ZipCode = ShoppingCartVM.ReservationHeader.ApplicationUser.ZipCode;

            Random random = new Random();
            var randval = random.Next(10000,99999);
            ShoppingCartVM.ReservationHeader.TrackingNumber = randval.ToString();

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price);
                ShoppingCartVM.ReservationHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPOST(IFormFile file)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;

            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Product");

            

            ShoppingCartVM.ReservationHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.ReservationHeader.OrderStatus = SD.StatusPending;
            ShoppingCartVM.ReservationHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.ReservationHeader.ShippingDate = ShoppingCartVM.ReservationHeader.PreferredDate;
            ShoppingCartVM.ReservationHeader.ApplicationUserId = claim.Value;

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price);
                ShoppingCartVM.ReservationHeader.OrderTotal += (cart.Price * cart.Count);
            }

            string wwwRootPath = _hostEnvironment.WebRootPath;
            if (file != null)
            {
                ShoppingCartVM.ReservationHeader.PaymentDate = System.DateTime.Now;
                string fileName = Guid.NewGuid().ToString();
                var uploads = Path.Combine(wwwRootPath, @"images\gcash");
                var extension = Path.GetExtension(file.FileName);

                using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    file.CopyTo(fileStreams);
                }
                ShoppingCartVM.ReservationHeader.GCashImageUrl = @"\images\gcash\" + fileName + extension;

            }

            
            _unitOfWork.ReservationHeader.Add(ShoppingCartVM.ReservationHeader);
            _unitOfWork.Save();
            
            foreach (var cart in ShoppingCartVM.ListCart)
            {
                ReservationDetail reservationDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = ShoppingCartVM.ReservationHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.ReservationDetail.Add(reservationDetail);
                _unitOfWork.Save();
            }

            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
            _unitOfWork.Save();
            return RedirectToAction("ReservationConfirmation", "Cart", new { id = ShoppingCartVM.ReservationHeader.Id });
        }

        public IActionResult ReservationConfirmation(int id)
        {
            ReservationHeader reservationHeader = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == id, includeProperties: "ApplicationUser");
            _emailSender.SendEmailAsync(reservationHeader.ApplicationUser.Email, "Reservation Confirmed! - Meatify", $"<p>Thank you for making a reservation, {reservationHeader.FirstName}! This is for Reservation # {id}.</p>");
            return View(id);
        }

        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.IncrementCount(cart, 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            
            if(cart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
            }
            else
            {
                _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBasedOnQuantity(double quantity, double price)
        {
            return price;
        }

        public IActionResult SendOtp(string id,string otp)
        {
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u=> u.Id == id);
            _emailSender.SendEmailAsync(applicationUser.Email, "Your Reservation OTP! - Meatify", $"<p>Thank you for making a reservation, {applicationUser.FirstName}! Your OTP is {otp}.</p>");
            return Json(new { success = true });
        }
    }
}
