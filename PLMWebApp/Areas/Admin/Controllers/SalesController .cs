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
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace PLMWebApp.Controllers;
[Area("Admin")]
[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Operation + "," + SD.Role_Logistics + "," + SD.Role_Sales)]
public class SalesController : Controller
{
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<IdentityUser> _userManager;

    public SalesController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, IEmailSender emailSender)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _emailSender = emailSender;
    }


    public IActionResult Index()
    {
        return View();
    }

    public IActionResult SalesItems(int id)
    {
        SalesReport sales = _unitOfWork.SalesReport.GetFirstOrDefault(u => u.Id == id);
        return View(sales);
    }
    public bool ValidateRole(string email, string role)
    {
        var user = _userManager.FindByEmailAsync(email).Result;
        return _userManager.IsInRoleAsync(user, role).Result;
    }

    public IActionResult Details(int reservationId, int oid)
    {
        ReservationVM reservationVM = new ReservationVM()
        {
            ReservationHeader = _unitOfWork.ReservationHeader.GetFirstOrDefault(u => u.Id == reservationId, includeProperties: "ApplicationUser"),
            ReservationDetail = _unitOfWork.ReservationDetail.GetAll(u => u.OrderId == reservationId, includeProperties: "Batch,Batch.Product"),
            Carrier = _unitOfWork.ApplicationUser.GetAll().Where(u => ValidateRole(u.Email, SD.Role_Courier)).Select(i => new SelectListItem
            {
                Text = i.Email,
                Value = i.Id.ToString()
            }),
            oid = oid,
        };

        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        ApplicationUser user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
        ReservationViewed view = _unitOfWork.ReservationViewed.GetFirstOrDefault(u => u.OrderId == reservationId && u.AlertEmail == user.Email);
        if (view != null)
        {
            _unitOfWork.ReservationViewed.Remove(view);
            _unitOfWork.Save();
        }
        return View(reservationVM);
    }

    //GET
    public IActionResult Upsert(int? id)
    {
        SalesReport sales = new();

        if (id == null || id == 0)
        {
            //create
            return View(sales);
        }
        else
        {
            //update
            sales = _unitOfWork.SalesReport.GetFirstOrDefault(u => u.Id == id);
            return View(sales);
        }

    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(SalesReport sales)
    {
        if (ModelState.IsValid)
        {
            if (sales.Id == 0)
            {
                IEnumerable<ReservationHeader> salesItem = _unitOfWork.ReservationHeader.GetAll(u => u.OrderStatus == SD.StatusCompleted).Where(u => u.ShippingDate >= sales.MinDate).Where(u => u.ShippingDate <= sales.MaxDate);
                sales.ReservationAmount = salesItem.Count();
                sales.BaseCosts = 0;
                foreach (var head in salesItem)
                {
                    sales.BaseCosts += head.BaseTotal;
                };
                sales.GrossIncome = 0;
                foreach (var head in salesItem)
                {
                    sales.GrossIncome += head.OrderTotal;
                };
                sales.NetIncome = sales.GrossIncome - sales.BaseCosts - sales.Overhead;
                DateTime today = DateTime.Now;
                sales.GenerationDate = new DateTime(today.Year, today.Month, today.Day, today.Hour, today.Minute, today.Second, today.Kind);
                _unitOfWork.SalesReport.Add(sales);
                TempData["Success"] = "Report Generated Successfully";
            }
            else
            {
                IEnumerable<ReservationHeader> salesItem = _unitOfWork.ReservationHeader.GetAll(u => u.OrderStatus == SD.StatusCompleted).Where(u => u.ShippingDate >= sales.MinDate).Where(u => u.ShippingDate <= sales.MaxDate);
                sales.ReservationAmount = 0;
                foreach (var head in salesItem)
                {
                    sales.ReservationAmount += 1;
                };
                sales.BaseCosts = 0;
                foreach (var head in salesItem)
                {
                    sales.BaseCosts += head.BaseTotal;
                };
                sales.GrossIncome = 0;
                foreach (var head in salesItem)
                {
                    sales.GrossIncome += head.OrderTotal;
                };
                sales.NetIncome = sales.GrossIncome - sales.BaseCosts - sales.Overhead;
                DateTime today = DateTime.Now;
                sales.GenerationDate = new DateTime(today.Year, today.Month, today.Day, today.Hour, today.Minute, today.Second, today.Kind);
                _unitOfWork.SalesReport.Update(sales);
                TempData["Success"] = "Report Updated Successfully";
            }

            _unitOfWork.Save();

            return RedirectToAction("Index");
        }
        return View(sales);
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
    public IActionResult GetAll()
    {
        var salesList = _unitOfWork.SalesReport.GetAll();
        return Json(new { data = salesList });
    }

    public IActionResult GetAll2(int id)
    {
        SalesReport sales = _unitOfWork.SalesReport.GetFirstOrDefault(u => u.Id == id);
        IEnumerable<ReservationHeader> reservationHeaders = _unitOfWork.ReservationHeader.GetAll(u => u.OrderStatus == SD.StatusCompleted,includeProperties:("ApplicationUser")).Where(u => u.ShippingDate >= sales.MinDate).Where(u => u.ShippingDate <= sales.MaxDate);

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

            _unitOfWork.Save();
        };

        return Json(new { data = reservationHeaders });
    }

    //POST
    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        var obj = _unitOfWork.SalesReport.GetFirstOrDefault(u => u.Id == id);
        if (obj == null)
        {
            return Json(new { success = false, message = "Error while deleting" });
        }
        _unitOfWork.SalesReport.Remove(obj);
        _unitOfWork.Save();
        return Json(new { success = true, message = "Report Deleted Successfully" });
    }

    #endregion
}

