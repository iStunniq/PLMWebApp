using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLM.DataAccess.Repository.IRepository;
using PLM.Models;
using PLM.Models.ViewModels;
using PLM.Utility;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;

namespace PLMWebApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        
        public int OrderTotal { get; set; }
        
        public CartController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
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

            ShoppingCartVM.ReservationHeader.FirstName = ShoppingCartVM.ReservationHeader.ApplicationUser.FirstName;
            ShoppingCartVM.ReservationHeader.LastName = ShoppingCartVM.ReservationHeader.ApplicationUser.LastName;
            ShoppingCartVM.ReservationHeader.Phone = ShoppingCartVM.ReservationHeader.ApplicationUser.Phone;
            ShoppingCartVM.ReservationHeader.Address = ShoppingCartVM.ReservationHeader.ApplicationUser.Address;
            ShoppingCartVM.ReservationHeader.City = ShoppingCartVM.ReservationHeader.ApplicationUser.City;
            ShoppingCartVM.ReservationHeader.ZipCode = ShoppingCartVM.ReservationHeader.ApplicationUser.ZipCode;

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
    }
}
