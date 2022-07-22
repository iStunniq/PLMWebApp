using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLM.DataAccess.Repository.IRepository;
using PLM.Models;
using PLM.Models.ViewModels;
using System.Diagnostics;
using System.Security.Claims;

namespace PLMWebApp.Controllers;
[Area("Customer")]
public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IUnitOfWork _unitOfWork;
        
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

    public IActionResult Index(int? cid, int? bid)
    {
        if (cid != null && bid != null) {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(u => u.IsActive, includeProperties: "Category,Brand").Where(u => u.Stock > 0).Where(u => u.BrandId == bid).Where(u => u.CategoryId == cid);
            return View(productList);
        } else if (cid != null && bid == null) {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(u => u.IsActive, includeProperties: "Category,Brand").Where(u => u.Stock > 0).Where(u => u.CategoryId == cid);
            return View(productList);
        } else if (cid == null && bid != null) {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(u => u.IsActive, includeProperties: "Category,Brand").Where(u => u.Stock > 0).Where(u => u.BrandId == bid);
            return View(productList);
        } else {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(u => u.IsActive, includeProperties: "Category,Brand").Where(u => u.Stock > 0);
            return View(productList);
        }
    }

        public IActionResult Details(int productId)
        {
        // ShoppingCart cartObj = new()
        //{
        //    Count = 1,
        //    ProductId = productId,
        //    Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == productId, includeProperties: "Category,Brand")
        //};

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
