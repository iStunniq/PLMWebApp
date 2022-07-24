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
public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SeeAll()
        {
            return View();
        }

    //GET
    public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                Product = new(),
                CategoryList = _unitOfWork.Category.GetAll(u=>u.IsActive).Select(i=>new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                BrandList = _unitOfWork.Brand.GetAll(u => u.IsActive).Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };    

            if (id==null || id == 0)
            {
                //Create Product
                //ViewBag.CategoryList = CategoryList;
                //ViewData["BrandList"] = BrandList;
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
                //Update Product
                return View(productVM);
            }
            
            
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);
                    
                    if (obj.Product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                        System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }

                    obj.Product.ImageUrl = @"\images\products\" + fileName + extension;
                }

                if (obj.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(obj.Product);
                    TempData["Success"] = "Product Created Successfully";
                }
                else
                {
                    _unitOfWork.Product.Update(obj.Product);
                    TempData["Success"] = "Product Updated Successfully";
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
        var productList = _unitOfWork.Product.GetAll(u=>u.IsActive,includeProperties:"Category,Brand");
        return Json(new { data = productList });
    }
    public IActionResult GetAll2()
    {
        var productList = _unitOfWork.Product.GetAll(includeProperties: "Category,Brand");
        return Json(new { data = productList });
    }

    //POST
    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        var obj = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
        if (obj == null)
        {
            return Json(new { success = false, message = "Error while deactivating" });
        }

        //var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
        //if (System.IO.File.Exists(oldImagePath))
        //{
        //    System.IO.File.Delete(oldImagePath);
        //}
        obj.IsActive = false;
        obj.Inactivity = DateTime.Now;
        _unitOfWork.Product.Update(obj);
        _unitOfWork.Save();
        return Json(new { success = true, message = "Product Deactivated Successfully" });
        //return RedirectToAction("Index");

    }

    public IActionResult Activate(int? id)
    {
        var obj = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
        if (obj == null)
        {
            return Json(new { success = false, message = "Error while Reactivating" });
        }

        //var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
        //if (System.IO.File.Exists(oldImagePath))
        //{
        //    System.IO.File.Delete(oldImagePath);
        //}
        obj.IsActive = true;
        _unitOfWork.Product.Update(obj);
        _unitOfWork.Save();
        return Json(new { success = true, message = "Product Reactivated Successfully" });
        //return RedirectToAction("Index");

    }

    #endregion
}

