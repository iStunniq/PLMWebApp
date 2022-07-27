using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PLM.DataAccess;
using PLM.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Hosting;
using PLM.Models;
using PLM.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using PLM.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace PLMWebApp.Controllers;
[Area("Admin")]
[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Operation)]
public class InventoryController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly IEmailSender _emailSender;
    private readonly UserManager<IdentityUser> _userManager;
    public InventoryController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment, UserManager<IdentityUser> userManager, IEmailSender emailSender)
    {
        _unitOfWork = unitOfWork;
        _hostEnvironment = hostEnvironment;
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

    public IActionResult Batches(int id)
    {
        InventoryVM inventoryVM = new();
        inventoryVM.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
        inventoryVM.Batchlist = _unitOfWork.Batch.GetAll(u => u.Product == inventoryVM.Product).Where(u => u.Stock > 0);
        return View(inventoryVM);
    }

    public IActionResult SeeAll(int id)
    {
        InventoryVM inventoryVM = new();
        inventoryVM.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
        inventoryVM.Batchlist = _unitOfWork.Batch.GetAll(u => u.Product == inventoryVM.Product);
        return View(inventoryVM);
    }

    //GET
    public IActionResult Upsert(int prodid, int? batchid)
    {
        InventoryVM inventoryVM = new()
        {
            Product = new(),
            Batch = new()
        };
        if (batchid == null || batchid == 0)
        {
            inventoryVM.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == prodid);
            return View(inventoryVM);
        }
        else
        {
            inventoryVM.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == prodid);
            inventoryVM.Batch = _unitOfWork.Batch.GetFirstOrDefault(u => u.Id == batchid);
            //Update Product
            return View(inventoryVM);
        }
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(InventoryVM obj)
    {
        if (obj.Batch.Id == 0)
        {
            _unitOfWork.Batch.Add(obj.Batch);
            TempData["Success"] = "Batch Created Successfully";
            IEnumerable<ApplicationUser> opeEmployees = _unitOfWork.ApplicationUser.GetAll().Where(u => ValidateRole(u.Email, SD.Role_Operation));

            foreach (var man in opeEmployees)
            {
                _emailSender.SendEmailAsync(man.Email, $"New Batch for {obj.Product.Name} - Meatify", $"<p><h4>{man.FirstName}, please check inventory for Product: <i>{obj.Product.Name}</i>. A new batch has been added, and it's Stock is equal to: {obj.Batch.Stock}. Thank you!</h4></p>");
            };
        }
        else
        {
            if (_unitOfWork.Batch.GetFirstOrDefault(u => u.Id == obj.Batch.Id).BasePrice != obj.Batch.BasePrice)
            {
                _unitOfWork.Batch.Update(obj.Batch);
                IEnumerable<SalesReport> salesReport = _unitOfWork.SalesReport.GetAll();
                IEnumerable<ReservationHeader> reservationHeaders = _unitOfWork.ReservationHeader.GetAll();

                foreach (var reservation in reservationHeaders)
                {
                    IEnumerable<ReservationDetail> reservationDetails = _unitOfWork.ReservationDetail.GetAll(u => u.OrderId == reservation.Id,includeProperties:"Batch");
                    reservation.BaseTotal = 0;
                    foreach (var detail in reservationDetails) {
                        reservation.BaseTotal += detail.Count * detail.Batch.BasePrice;
                    }
                }
                _unitOfWork.Save();
                foreach (SalesReport sales in salesReport)
                {
                    IEnumerable<ReservationHeader> salesItems = _unitOfWork.ReservationHeader.GetAll(u => u.OrderStatus == SD.StatusCompleted).Where(u => u.ShippingDate >= sales.MinDate).Where(u => u.ShippingDate <= sales.MaxDate);
                    sales.BaseCosts = 0;
                    foreach (var item in salesItems)
                    {
                        sales.BaseCosts += item.BaseTotal;
                    }
                    sales.NetIncome = sales.GrossIncome - sales.BaseCosts - sales.Overhead;
                }
                _unitOfWork.Save();
            } else
            {
                _unitOfWork.Batch.Update(obj.Batch);
            }
            TempData["Success"] = "Batch Updated Successfully";
            IEnumerable<ApplicationUser> opeEmployees = _unitOfWork.ApplicationUser.GetAll().Where(u => ValidateRole(u.Email, SD.Role_Operation));

            foreach (var man in opeEmployees)
            {
                _emailSender.SendEmailAsync(man.Email, $"Updated Batch for {obj.Product.Name} - Meatify", $"<p><h4>{man.FirstName}, please check inventory for Product: <i>{obj.Product.Name}</i>, Batch ID: {obj.Batch.Id}. It was recently updated, and this batch's Stock is now equal to: {obj.Batch.Stock}. Thank you!</h4></p>");
            };
        }
        _unitOfWork.Save();
        Product prod = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == obj.Product.Id);
        _unitOfWork.Product.Update(prod);
        _unitOfWork.Save();

        return RedirectToAction("Batches", new { prod.Id });
    }

    //GET
    //public IActionResult Delete(int? id)
    //{
    //    if (id == null || id == 0)
    //    {
    //        return NotFound();
    //    }

    //    var productFromDbFirst = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);

    //    if (productFromDbFirst == null)
    //    {
    //        return NotFound();
    //    }

    //    return View(productFromDbFirst);
    //}



    #region API CALLS
    [HttpGet]

    public IActionResult GetProducts()
    {
        var productList = _unitOfWork.Product.GetAll(u => u.IsActive, includeProperties: "Category,Brand");
        return Json(new { data = productList });
    }
    public IActionResult GetAll(int id)
    {
        var batchList = _unitOfWork.Batch.GetAll(u => u.ProductId == id, includeProperties: "Product").Where(u => u.Stock > 0);
        return Json(new { data = batchList });
    }
    public IActionResult GetAll2(int id)
    {
        var batchList = _unitOfWork.Batch.GetAll(u => u.ProductId == id, includeProperties: "Product");
        return Json(new { data = batchList });
    }

    #endregion
}

