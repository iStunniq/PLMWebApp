using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    public IActionResult Index(int? cid, int? bid, int? min, int? max)
    {
        HomeVM homeVM = new();
        homeVM.Products = _unitOfWork.Product.GetAll(u => u.IsActive && u.Brand.IsActive && u.Category.IsActive, includeProperties: "Category,Brand").Where(u => u.Stock > 0);
        if (cid != null && cid != 0)
        {
            homeVM.Products = homeVM.Products.Where(u => u.CategoryId == cid);
        }
        if (bid != null && bid != 0)
        {
            homeVM.Products = homeVM.Products.Where(u => u.BrandId == bid);
        }
        if (min != null)
        {
            homeVM.Products = homeVM.Products.Where(u => u.Price >= min);
        }
        if (max != null)
        {
            homeVM.Products = homeVM.Products.Where(u => u.Price <= max);
        }

        homeVM.CategoryList = _unitOfWork.Category.GetAll(u => u.IsActive).Select(i => new SelectListItem
        {
            Text = i.Name,
            Value = i.Id.ToString()
        });
        homeVM.BrandList = _unitOfWork.Brand.GetAll(u => u.IsActive).Select(i => new SelectListItem
        {
            Text = i.Name,
            Value = i.Id.ToString()
        }) ;

        if (_signInManager.IsSignedIn(User))
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ApplicationUser user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);

            homeVM.Alert = _unitOfWork.ReservationViewed.GetAll(u => u.AlertEmail == user.Email).Count();
            if (User.IsInRole(SD.Role_Operation))
            {
                homeVM.Alert = _unitOfWork.Product.GetAll(u => u.Stock < SD.Mid && u.IsActive).Count();
            }
        }
        return View(homeVM);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(HomeVM obj)
    {
        return RedirectToAction("Index", new { cid = obj.CategoryId, bid = obj.BrandId, min=obj.MinPrice,max=obj.MaxPrice });
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
                cartObj.Expiry = _unitOfWork.Batch.GetFirstOrDefault(u => u.Stock > 0 && u.ProductId == productId).Expiry;
                return View(cartObj);
            }
            cartFromDb.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == productId, includeProperties: "Category,Brand");
            cartFromDb.Expiry = _unitOfWork.Batch.GetFirstOrDefault(u => u.Stock > 0 && u.ProductId == productId).Expiry;
            return View(cartFromDb);
        } else
        {
            ShoppingCart cartObj = new()
            {
                Count = 1,
                ProductId = productId,
                Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == productId, includeProperties: "Category,Brand")
            };
            cartObj.Expiry = _unitOfWork.Batch.GetFirstOrDefault(u => u.Stock > 0 && u.ProductId == productId).Expiry;
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
        [HttpGet]
        public IActionResult Validate()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ApplicationUser user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            if (user.EmailConfirmed)
            {
                return Json(new { success = true });
            }
            else {
                return Json(new { success = false});
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
