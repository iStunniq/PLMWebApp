using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PLM.DataAccess;
using PLM.DataAccess.Repository.IRepository;
using PLM.Models;
using PLM.Utility;

namespace PLMWebApp.Controllers;
[Area("Admin")]
[Authorize(Roles = SD.Role_Admin +","+ SD.Role_Operation + "," + SD.Role_Marketing)]
public class BrandController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public BrandController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Brand> objBrandList = _unitOfWork.Brand.GetAll(u=>u.IsActive);
            return View(objBrandList);
        }
        public IActionResult SeeAll()
        {
            IEnumerable<Brand> objBrandList = _unitOfWork.Brand.GetAll();
            return View(objBrandList);
        }

    //GET
    public IActionResult Create()
        {
            
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Brand obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Brand.Add(obj);
                _unitOfWork.Save();
                TempData["Success"] = "Brand Created Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        //GET
        public IActionResult Edit(int? id)
        {
            if(id==null || id == 0)
            {
                return NotFound();
            }
            var brandFromDbFirst = _unitOfWork.Brand.GetFirstOrDefault(u => u.Id == id);

            if(brandFromDbFirst == null)
            {
                return NotFound();
            }

            return View(brandFromDbFirst);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Brand obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Brand.Update(obj);
                _unitOfWork.Save();
                TempData["Success"] = "Brand Updated Successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        //GET
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var brandFromDbFirst = _unitOfWork.Brand.GetFirstOrDefault(u => u.Id == id);

            if (brandFromDbFirst == null)
            {
                return NotFound();
            }

            return View(brandFromDbFirst);
        }

        //POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _unitOfWork.Brand.GetFirstOrDefault(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }

            obj.IsActive = false;
            obj.Inactivity = DateTime.Now;
            _unitOfWork.Brand.Update(obj);
            _unitOfWork.Save();
            TempData["Success"] = "Brand Deleted Successfully";
            return RedirectToAction("Index");

        }

    public IActionResult Activate(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }
        var BrandFromDbFirst = _unitOfWork.Brand.GetFirstOrDefault(u => u.Id == id);

        if (BrandFromDbFirst == null)
        {
            return NotFound();
        }

        return View(BrandFromDbFirst);
    }

    //POST
    [HttpPost, ActionName("Activate")]
    [ValidateAntiForgeryToken]
    public IActionResult ActivatePOST(int? id)
    {
        var obj = _unitOfWork.Brand.GetFirstOrDefault(u => u.Id == id);
        if (obj == null)
        {
            return NotFound();
        }
        obj.IsActive = true;
        _unitOfWork.Brand.Update(obj);
        _unitOfWork.Save();
        TempData["Success"] = "Brand Activated Successfully";
        return RedirectToAction("SeeAll");
    }
}

