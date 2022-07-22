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
[Authorize(Roles = SD.Role_Admin)]
public class InventoryController : Controller
    {
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _hostEnvironment;
    public InventoryController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _hostEnvironment = hostEnvironment;
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
    public IActionResult Upsert(int prodid,int? batchid)
        {
        InventoryVM inventoryVM = new()
        {
            Product = new(),
            Batch = new()
        };
            if (batchid==null || batchid == 0)
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
                }
                else
                {
                    _unitOfWork.Batch.Update(obj.Batch);
                    TempData["Success"] = "Batch Updated Successfully";
                }
                _unitOfWork.Save();
                Product prod = _unitOfWork.Product.GetFirstOrDefault(u=>u.Id==obj.Product.Id);
                IEnumerable<Batch> batches = _unitOfWork.Batch.GetAll(u => u.ProductId == prod.Id);
                var stock = 0;
                foreach (Batch batch in batches)
                {
                stock = stock + batch.Stock;
                }
                prod.Stock = stock;

            _unitOfWork.Product.Update(prod);
        _unitOfWork.Save();

        return RedirectToAction("Batches",new {prod.Id});
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
        var batchList = _unitOfWork.Batch.GetAll(u=>u.ProductId==id , includeProperties:"Product").Where(u=>u.Stock>0);
        return Json(new { data = batchList });
    }
    public IActionResult GetAll2(int id)
    {
        var batchList = _unitOfWork.Batch.GetAll(u=>u.ProductId==id ,includeProperties: "Product");
        return Json(new { data = batchList });
    }

    #endregion
}

