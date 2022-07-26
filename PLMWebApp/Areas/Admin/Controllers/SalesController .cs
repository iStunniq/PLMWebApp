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

namespace PLMWebApp.Controllers;
[Area("Admin")]
[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Operation)]
public class SalesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public SalesController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

    //GET
    public IActionResult Upsert(int? id)
        {
        SalesReport sales = new();  

            if (id==null || id == 0)
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
                    IEnumerable<ReservationHeader> salesItem = _unitOfWork.ReservationHeader.GetAll(u=>u.OrderStatus==SD.StatusCompleted).Where(u=>u.ShippingDate>=sales.MinDate).Where(u => u.ShippingDate <= sales.MaxDate);
                    sales.ReservationAmount = salesItem.Count();
                    sales.BaseCosts = 0;
                    foreach (var head in salesItem) {
                        IEnumerable<ReservationDetail> headItem = _unitOfWork.ReservationDetail.GetAll(u => u.OrderId == head.Id,includeProperties:("Batch"));
                        foreach (var detail in headItem)
                        {
                            sales.BaseCosts += detail.Batch.BasePrice * detail.Count;
                        };
                    };
                    sales.GrossIncome = 0;
                    foreach (var head in salesItem)
                    {
                        sales.GrossIncome += head.OrderTotal;
                    };
                    sales.NetIncome = sales.GrossIncome - sales.BaseCosts - sales.Overhead;
                    sales.GenerationDate = DateTime.Now;
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
                        IEnumerable<ReservationDetail> headItem = _unitOfWork.ReservationDetail.GetAll(u => u.OrderId == head.Id, includeProperties: ("Batch"));
                        foreach (var detail in headItem)
                        {
                            sales.BaseCosts += detail.Batch.BasePrice * detail.Count;
                        };
                    };
                    sales.GrossIncome = 0;
                    foreach (var head in salesItem)
                    {
                        sales.GrossIncome += head.OrderTotal;
                    };
                    sales.NetIncome = sales.GrossIncome - sales.BaseCosts - sales.Overhead;
                    sales.GenerationDate = DateTime.Now;
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

    //POST
    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        var obj = _unitOfWork.SalesReport.GetFirstOrDefault(u => u.Id == id);
        if (obj == null)
        {
            return Json(new { success = false, message = "Error while deleting" });
        }

        //var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
        //if (System.IO.File.Exists(oldImagePath))
        //{
        //    System.IO.File.Delete(oldImagePath);
        //}
        _unitOfWork.SalesReport.Remove(obj);
        _unitOfWork.Save();
        return Json(new { success = true, message = "Report Deleted Successfully" });
        //return RedirectToAction("Index");

    }

    #endregion
}

