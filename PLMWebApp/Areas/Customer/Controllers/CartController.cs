using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLM.DataAccess.Repository.IRepository;
using PLM.Models;
using PLM.Models.ViewModels;
using PLM.Utility;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;

namespace PLMWebApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;


        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment, IEmailSender emailSender, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public bool ValidateRole(string email, string role)
        {
            var user = _userManager.FindByEmailAsync(email).Result;
            return _userManager.IsInRoleAsync(user, role).Result;
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

            ShoppingCartVM.ReservationHeader.OrderTotal = 0;
            foreach (var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Count, cart.Product.Price);
                ShoppingCartVM.ReservationHeader.OrderTotal += (cart.Price * cart.Count);
            }

            string wwwRootPath = _hostEnvironment.WebRootPath;
            if (file != null && !ShoppingCartVM.ReservationHeader.COD)
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

            if (ShoppingCartVM.ReservationHeader.COD)
            {
                ShoppingCartVM.ReservationHeader.PaymentDueDate = ShoppingCartVM.ReservationHeader.PreferredDate;
                ShoppingCartVM.ReservationHeader.GCashImageUrl = @"\images\CODDefault.png";
            }

            IEnumerable<ApplicationUser> SalesEmployees = _unitOfWork.ApplicationUser.GetAll().Where(u => ValidateRole(u.Email, SD.Role_Sales));
            foreach (var man in SalesEmployees)
            {
                ReservationViewed view = new();
                view.OrderId = ShoppingCartVM.ReservationHeader.Id;
                view.AlertEmail = man.Email;
                _unitOfWork.ReservationViewed.Add(view);
            }
            ShoppingCartVM.ReservationHeader.BaseTotal = 0;
            _unitOfWork.ReservationHeader.Add(ShoppingCartVM.ReservationHeader);
            _unitOfWork.Save();
            foreach (var cart in ShoppingCartVM.ListCart)
            {
                while (cart.Count > 0)
                {
                    Batch batch = _unitOfWork.Batch.GetFirstOrDefault(u => u.ProductId == cart.ProductId && u.Stock > 0);
                    if (batch.Stock >= cart.Count)
                    {
                        ReservationDetail reservationDetail = new()
                        {
                            BatchId = batch.Id,
                            OrderId = ShoppingCartVM.ReservationHeader.Id,
                            Price = GetPriceBasedOnQuantity(cart.Count,cart.Product.Price),
                            Count = cart.Count
                        };
                        ShoppingCartVM.ReservationHeader.BaseTotal += batch.BasePrice * cart.Count;
                        batch.Stock = batch.Stock - cart.Count;
                        cart.Count = 0;
                        _unitOfWork.ReservationDetail.Add(reservationDetail);
                    }
                    else
                    {
                        ReservationDetail reservationDetail = new()
                        {
                            BatchId = batch.Id,
                            OrderId = ShoppingCartVM.ReservationHeader.Id,
                            Price = GetPriceBasedOnQuantity(batch.Stock, cart.Product.Price),
                            Count = batch.Stock
                        };
                        ShoppingCartVM.ReservationHeader.BaseTotal += batch.BasePrice * batch.Stock;
                        cart.Count = cart.Count - batch.Stock;
                        batch.Stock = 0;
                        _unitOfWork.ReservationDetail.Add(reservationDetail);
                    }
                    _unitOfWork.Save();
                }
                _unitOfWork.Product.Update(cart.Product);
                _unitOfWork.Save();
            }
            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
            _unitOfWork.Save();
            return RedirectToAction("ReservationConfirmation", "Cart", new { id = ShoppingCartVM.ReservationHeader.Id });
        }

        public IActionResult ReservationConfirmation(int id)
        {
            ReservationHeader reservationHeader = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == id, includeProperties: "ApplicationUser");
            _emailSender.SendEmailAsync(reservationHeader.ApplicationUser.Email, "Reservation Confirmed! - Meatify", $"<p><h3>Thank you for making a reservation, {reservationHeader.FirstName}! " +
                $"This is for Reservation # {id}. </p> <p>Your reservation is now in the Pending tab, go to Reservations.</h3></p> <p><em> NOTICE: Cancelling reservations is handled by our Meatify Staff directly</em></p>" +
                $"<p><em>Please contact details for more information: {SD.Contact}</em></p>");
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
            var claimsIdentity = (ClaimsIdentity)User.Identity;

            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<ShoppingCart> ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product");

            foreach (var item in ListCart)
            {
                Product product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == item.ProductId, includeProperties: ("Brand,Category"));
                if (item.Count > product.Stock)
                {
                    return Json(new { success = false });
                };
                if (!product.IsActive || !product.Brand.IsActive || !product.Category.IsActive)
                {
                    return Json(new { success = false });
                };
            }

            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u=> u.Id == id);
            _emailSender.SendEmailAsync(applicationUser.Email, "Your Reservation OTP! - Meatify", $"<p>Thank you for making a reservation, {applicationUser.FirstName}! Your OTP is {otp}.</p>");
            return Json(new { success = true });
        }
        public IActionResult ValidateStock()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;

            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            IEnumerable<ShoppingCart> ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product");

            foreach (var item in ListCart)
            {
                Product product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == item.ProductId,includeProperties:("Brand,Category"));
                if (item.Count > product.Stock)
                {
                    return Json(new { success = false });
                };
                if (!product.IsActive || !product.Brand.IsActive || !product.Category.IsActive)
                {
                    return Json(new { success = false });
                };
            }
            return Json(new { success = true });
        }
    }
}
