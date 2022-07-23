using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PLM.DataAccess.Repository.IRepository;
using PLM.Models;
using PLM.Models.ViewModels;
using PLM.Utility;
using System.Diagnostics;
using System.Security.Claims;

namespace PLMWebApp.Controllers;
[Area("Customer")]
public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, SignInManager<IdentityUser> signInManager)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _signInManager = signInManager;
        }

    public IActionResult Index(int? cid, int? bid)
    {
        HomeVM homeVM = new();
        if (cid != null && bid != null) {
            homeVM.Products = _unitOfWork.Product.GetAll(u => u.IsActive, includeProperties: "Category,Brand").Where(u => u.Stock > 0).Where(u => u.BrandId == bid).Where(u => u.CategoryId == cid);
        } else if (cid != null && bid == null) {
            homeVM.Products = _unitOfWork.Product.GetAll(u => u.IsActive, includeProperties: "Category,Brand").Where(u => u.Stock > 0).Where(u => u.CategoryId == cid);
        } else if (cid == null && bid != null) {
            homeVM.Products = _unitOfWork.Product.GetAll(u => u.IsActive, includeProperties: "Category,Brand").Where(u => u.Stock > 0).Where(u => u.BrandId == bid);
        } else {
            homeVM.Products = _unitOfWork.Product.GetAll(u => u.IsActive, includeProperties: "Category,Brand").Where(u => u.Stock > 0);
        }
        if (User.IsInRole(SD.Role_Courier)){
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            homeVM.Alert = _unitOfWork.ReservationHeader.GetAll(u => u.Carrier == claim.Value && u.OrderStatus == SD.StatusApproval).Count();
        }
        if (User.IsInRole(SD.Role_Sales))
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            homeVM.Alert = _unitOfWork.ReservationHeader.GetAll(u => u.OrderStatus == SD.StatusPending).Count();
        }
        if (User.IsInRole(SD.Role_Logistics))
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            homeVM.Alert = _unitOfWork.ReservationHeader.GetAll(u => u.OrderStatus == SD.StatusInProcess).Count();
        }
        return View(homeVM);
    }

        public IActionResult Details(int productId)
        {
        // ShoppingCart cartObj = new()
        //{
        //    Count = 1,
        //    ProductId = productId,
        //    Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == productId, includeProperties: "Category,Brand")
        //};

        if (_signInManager.IsSignedIn(User)){

            var claimsIdentity = (ClaimsIdentity)User.Identity;

            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.ApplicationUserId == claim.Value && u.ProductId == productId, includeProperties: "Product");

            if (cartFromDb == null)
            {
                ShoppingCart cartObj = new()
                {
                    Count = 1,
                    ProductId = productId,
                    Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == productId, includeProperties: "Category,Brand")
                };
                return View(cartObj);
            }
            cartFromDb.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == productId, includeProperties: "Category,Brand");
            return View(cartFromDb);
        } else
        {
            ShoppingCart cartObj = new()
            {
                Count = 1,
                ProductId = productId,
                Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == productId, includeProperties: "Category,Brand")
            };
            return View(cartObj);
        }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;

            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            shoppingCart.ApplicationUserId = claim.Value;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId,includeProperties:"Product");

            if (cartFromDb == null)
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            }
            else
            {
                cartFromDb.Count = shoppingCart.Count;
            }

            
            _unitOfWork.Save();

            return RedirectToAction("Index", "Cart");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
