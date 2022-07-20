using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PLM.DataAccess;
using PLM.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Hosting; 
using PLM.Models;
using PLM.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using PLM.Utility;

namespace PLMWebApp.Controllers;
[Area("Admin")]
[Authorize(Roles = SD.Role_Admin)]
public class RegionController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public RegionController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        //GET
        public IActionResult Upsert(int? id)
        {
            Region region = new();

            if (id==null || id == 0)
            {
                return View(region);
            }
            else
            {
                region = _unitOfWork.Region.GetFirstOrDefault(u => u.Id == id);
                //Update Region
                return View(region);
            }
            
            
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Region obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _unitOfWork.Region.Add(obj);
                    TempData["Success"] = "Region Created Successfully";
                }
                else
                {
                    _unitOfWork.Region.Update(obj);
                    TempData["Success"] = "Region Updated Successfully";
                }

                _unitOfWork.Save();
                
                return RedirectToAction("Index");
            }
            return View(obj);
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
        var regionList = _unitOfWork.Region.GetAll();
        return Json(new { data = regionList });
    }

    //POST
    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        var obj = _unitOfWork.Region.GetFirstOrDefault(u => u.Id == id);
        if (obj == null)
        {
            return Json(new { success = false, message = "Error while deleting" });
        }

        _unitOfWork.Region.Remove(obj);
        _unitOfWork.Save();
        return Json(new { success = true, message = "Region Deleted Successfully" });
        //return RedirectToAction("Index");

    }

    #endregion
}

