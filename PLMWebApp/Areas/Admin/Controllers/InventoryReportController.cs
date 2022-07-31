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
using OfficeOpenXml;

namespace PLMWebApp.Controllers;
[Area("Admin")]
[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Operation + "," + SD.Role_Logistics + "," + SD.Role_Sales)]
public class InventoryReportController : Controller
{
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<IdentityUser> _userManager;

    public InventoryReportController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, IEmailSender emailSender)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _emailSender = emailSender;
    }


    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Products(int id)
    {
        InventoryReport Report = _unitOfWork.InventoryReport.GetFirstOrDefault(u => u.Id == id);
        return View(Report);
    }

    public IActionResult Batches(int id, int pid)
    {
        InvReportDetail Product = _unitOfWork.InvReportDetail.GetFirstOrDefault(u => u.ReportId == id && u.ProductId == pid);
        return View(Product);
    }

    public bool ValidateRole(string email, string role)
    {
        var user = _userManager.FindByEmailAsync(email).Result;
        return _userManager.IsInRoleAsync(user, role).Result;
    }
    //GET
    public IActionResult Upsert(int? id)
    {
        InventoryReport Inventory = new();

        if (id == null || id == 0)
        {
            //create
            return View(Inventory);
        }
        else
        {
            //update
            Inventory = _unitOfWork.InventoryReport.GetFirstOrDefault(u => u.Id == id);
            return View(Inventory);
        }

    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(InventoryReport Inventory)
    {
        if (ModelState.IsValid)
        {
            if (Inventory.Id == 0)
            {
                DateTime today = DateTime.Now;
                Inventory.GenerationDate = new DateTime(today.Year, today.Month, today.Day, today.Hour, today.Minute, today.Second, today.Kind);
                _unitOfWork.InventoryReport.Add(Inventory);
                _unitOfWork.Save();
                IEnumerable<Product> products = _unitOfWork.Product.GetAll(u => u.IsActive, includeProperties: "Brand,Category");
                foreach (Product product in products)
                {
                    InvReportDetail detail = new InvReportDetail()
                    {
                        ReportId = Inventory.Id,
                        DetailType = "Product",
                        ProductBrand = product.Brand.Name,
                        ProductCategory = product.Category.Name,
                        ProductName = product.Name,
                        ProductId = product.Id,
                        ProductPrice = product.Price,
                        ProductExpiry = product.Expiry,
                        ProductStatus = product.StockStat,
                        ProductStock = product.Stock,
                        BatchBase = 0,
                        BatchExpiry = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                        BatchId = 0,
                        BatchStock = 0,
                    };
                    _unitOfWork.InvReportDetail.Add(detail);
                    IEnumerable<Batch> batches = _unitOfWork.Batch.GetAll(u => u.ProductId == product.Id && u.Stock > 0);
                    foreach (Batch batch in batches)
                    {
                        InvReportDetail Bdetail = new InvReportDetail()
                        {
                            ReportId = Inventory.Id,
                            DetailType = "Batch",
                            ProductBrand = product.Brand.Name,
                            ProductCategory = product.Category.Name,
                            ProductName = product.Name,
                            ProductId = product.Id,
                            ProductPrice = product.Price,
                            ProductExpiry = product.Expiry,
                            ProductStatus = product.StockStat,
                            ProductStock = product.Stock,
                            BatchBase = batch.BasePrice,
                            BatchExpiry = batch.Expiry,
                            BatchId = batch.Id,
                            BatchStock = batch.Stock
                        };
                        _unitOfWork.InvReportDetail.Add(Bdetail);
                    };
                }
                TempData["Success"] = "Report Generated Successfully";
            }
            else
            {
                _unitOfWork.InvReportDetail.RemoveRange(_unitOfWork.InvReportDetail.GetAll(u => u.ReportId == Inventory.Id));
                IEnumerable<Product> products = _unitOfWork.Product.GetAll(u => u.IsActive, includeProperties: "Brand,Category");
                foreach (Product product in products)
                {
                    InvReportDetail detail = new InvReportDetail()
                    {
                        ReportId = Inventory.Id,
                        DetailType = "Product",
                        ProductBrand = product.Brand.Name,
                        ProductCategory = product.Category.Name,
                        ProductName = product.Name,
                        ProductId = product.Id,
                        ProductPrice = product.Price,
                        ProductExpiry = product.Expiry,
                        ProductStatus = product.StockStat,
                        ProductStock = product.Stock,
                        BatchBase = 0,
                        BatchExpiry = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                        BatchId = 0,
                        BatchStock = 0
                    };
                    _unitOfWork.InvReportDetail.Add(detail);
                    IEnumerable<Batch> batches = _unitOfWork.Batch.GetAll(u => u.ProductId == product.Id && u.Stock > 0);
                    foreach (Batch batch in batches)
                    {
                        InvReportDetail Bdetail = new InvReportDetail()
                        {
                            ReportId = Inventory.Id,
                            DetailType = "Batch",
                            ProductBrand = product.Brand.Name,
                            ProductCategory = product.Category.Name,
                            ProductName = product.Name,
                            ProductId = product.Id,
                            ProductPrice = product.Price,
                            ProductExpiry = product.Expiry,
                            ProductStatus = product.StockStat,
                            ProductStock = product.Stock,
                            BatchBase = batch.BasePrice,
                            BatchExpiry = batch.Expiry,
                            BatchId = batch.Id,
                            BatchStock = batch.Stock
                        };
                        _unitOfWork.InvReportDetail.Add(Bdetail);
                    };
                }
                DateTime today = DateTime.Now;
                Inventory.GenerationDate = new DateTime(today.Year, today.Month, today.Day, today.Hour, today.Minute, today.Second, today.Kind);
                _unitOfWork.InventoryReport.Update(Inventory);
                TempData["Success"] = "Report Generated Successfully";
            }

            _unitOfWork.Save();

            return RedirectToAction("Index");
        }
        return View(Inventory);
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
        var InventoryList = _unitOfWork.InventoryReport.GetAll();
        return Json(new { data = InventoryList });
    }

    public IActionResult GetAll2(int id)
    {
        IEnumerable<InvReportDetail> Products = _unitOfWork.InvReportDetail.GetAll(u => u.DetailType == "Product" && u.ReportId == id);
        return Json(new { data = Products });
    }
    public IActionResult GetAll3(int id, int pid)
    {
        IEnumerable<InvReportDetail> Batches = _unitOfWork.InvReportDetail.GetAll(u => u.DetailType == "Batch" && u.ProductId == pid && u.ReportId == id);
        return Json(new { data = Batches });
    }
    public IActionResult Excel(int id)
    {
        InventoryReport report = _unitOfWork.InventoryReport.GetFirstOrDefault(u => u.Id == id);
        IEnumerable<InvReportDetail> Products = _unitOfWork.InvReportDetail.GetAll(u => u.DetailType == "Product" && u.ReportId == id);

        ExcelPackage pack = new ExcelPackage();
        ExcelWorksheet ws = pack.Workbook.Worksheets.Add("Report");

        ws.Cells["A1"].Value = "Inventory Report";
        ws.Cells["B1"].Value = report.Name;
        ws.Cells["A2"].Value = "Report Generated";
        ws.Cells["B2"].Value = report.GenerationDate.ToString();
        ws.Cells["A3"].Value = "Excel Generated";
        var time = DateTime.Now;
        ws.Cells["B3"].Value = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, time.Kind).ToString();
        ws.Cells["A5:J5"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        ws.Cells["A5:J5"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 84, 84));

        ws.Cells["A5"].Value = "Detail Type";
        ws.Cells["B5"].Value = "Product Name";
        ws.Cells["C5"].Value = "Brand";
        ws.Cells["D5"].Value = "Category";
        ws.Cells["E5"].Value = "Total Stock";
        ws.Cells["F5"].Value = "Batch Stock";
        ws.Cells["G5"].Value = "Stock Status";
        ws.Cells["H5"].Value = "Expiration";
        ws.Cells["I5"].Value = "Price";
        ws.Cells["J5"].Value = "Base Price";
        var Row = 6;
        foreach (var prod in Products)
        {
            ws.Cells["A" + Row].Value = prod.DetailType;
            ws.Cells["B" + Row].Value = prod.ProductName;
            ws.Cells["C" + Row].Value = prod.ProductBrand;
            ws.Cells["D" + Row].Value = prod.ProductCategory;
            ws.Cells["E" + Row].Value = prod.ProductStock;
            ws.Cells["F" + Row].Value = "Not a Batch";
            ws.Cells["G" + Row].Value = prod.ProductStatus;
            ws.Cells["H" + Row].Value = prod.ProductExpiry.ToString();
            ws.Cells["I" + Row].Value = prod.ProductPrice;
            ws.Cells["J" + Row].Value = "Not a Batch";
            Row++;
            IEnumerable<InvReportDetail> Batches = _unitOfWork.InvReportDetail.GetAll(u => u.DetailType == "Batch" && u.ProductId == prod.ProductId && u.ReportId == id);
            foreach (var batch in Batches)
            {
                ws.Cells["A" + Row].Value = batch.DetailType;
                ws.Cells["B" + Row].Value = batch.ProductName;
                ws.Cells["C" + Row].Value = batch.ProductBrand;
                ws.Cells["D" + Row].Value = batch.ProductCategory;
                ws.Cells["E" + Row].Value = batch.ProductStock;
                ws.Cells["F" + Row].Value = batch.BatchStock;
                ws.Cells["G" + Row].Value = batch.ProductStatus;
                ws.Cells["H" + Row].Value = batch.BatchExpiry.ToString();
                ws.Cells["I" + Row].Value = batch.ProductPrice;
                ws.Cells["J" + Row].Value = batch.BatchBase;
                Row++;
            }
            ws.Cells["A" + Row + ":J" + Row].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells["A" + Row + ":J" + Row].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 84, 84));

            Row++;
        }
        ws.Cells["A:AZ"].AutoFitColumns();
        ws.Cells["A5:J" + (Row - 1)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        ws.Cells["A5:J" + (Row - 1)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        ws.Cells["A5:J" + (Row - 1)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        ws.Cells["A5:J" + (Row - 1)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

        var fileDownloadName = "Inventory_" + report.GenerationDate + "_" + report.Name + ".xlsx";
        var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        var fileStream = new MemoryStream();
        pack.SaveAs(fileStream);
        fileStream.Position = 0;

        var fsr = new FileStreamResult(fileStream, contentType);
        fsr.FileDownloadName = fileDownloadName;

        return fsr;
    }

    //POST
    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        var obj = _unitOfWork.InventoryReport.GetFirstOrDefault(u => u.Id == id);
        if (obj == null)
        {
            return Json(new { success = false, message = "Error while deleting" });
        }
        _unitOfWork.InvReportDetail.RemoveRange(_unitOfWork.InvReportDetail.GetAll(u => u.ReportId == id));
        _unitOfWork.InventoryReport.Remove(obj);
        _unitOfWork.Save();
        return Json(new { success = true, message = "Report Deleted Successfully" });
    }

    #endregion
}

