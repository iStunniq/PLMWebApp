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
    public IActionResult SalesCancelled(int id)
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
    public IActionResult Details2(int reservationId, int oid)
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
                IEnumerable<ReservationHeader> cancelItem = _unitOfWork.ReservationHeader.GetAll(u => u.OrderStatus == SD.StatusCancelled).Where(u => u.CancelDate >= sales.MinDate).Where(u => u.CancelDate <= sales.MaxDate);

                sales.ReservationAmount = salesItem.Count();
                sales.CancelledAmount = cancelItem.Count();
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
        IEnumerable<ReservationHeader> reservationHeaders = _unitOfWork.ReservationHeader.GetAll(u => u.OrderStatus == SD.StatusCompleted, includeProperties: ("ApplicationUser")).Where(u => u.ShippingDate >= sales.MinDate).Where(u => u.ShippingDate <= sales.MaxDate);

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
    public IActionResult GetAll3(int id)
    {
        SalesReport sales = _unitOfWork.SalesReport.GetFirstOrDefault(u => u.Id == id);
        IEnumerable<ReservationHeader> reservationHeaders = _unitOfWork.ReservationHeader.GetAll(u => u.OrderStatus == SD.StatusCancelled, includeProperties: ("ApplicationUser")).Where(u => u.CancelDate >= sales.MinDate).Where(u => u.CancelDate <= sales.MaxDate);

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

    public IActionResult Excel(int id)
    {
        SalesReport report = _unitOfWork.SalesReport.GetFirstOrDefault(u => u.Id == id);
        IEnumerable<ReservationHeader> Reservations = _unitOfWork.ReservationHeader.GetAll(u => u.OrderStatus == SD.StatusCompleted, includeProperties: ("ApplicationUser")).Where(u => u.ShippingDate >= report.MinDate).Where(u => u.ShippingDate <= report.MaxDate);
        Reservations.OrderBy(u => u.ShippingDate);
        ExcelPackage pack = new ExcelPackage();
        ExcelWorksheet ws = pack.Workbook.Worksheets.Add("Report");

        ws.Cells["A1"].Value = "Sales Report";
        ws.Cells["B1"].Value = report.Name;
        ws.Cells["C1"].Value = "Amount Completed";
        ws.Cells["D1"].Value = report.ReservationAmount;
        ws.Cells["A2"].Value = "Report Generated";
        ws.Cells["B2"].Value = report.GenerationDate.ToString();
        ws.Cells["C2"].Value = "Amount Cancelled";
        ws.Cells["D2"].Value = report.CancelledAmount;
        ws.Cells["A3"].Value = "Excel Generated";
        var time = DateTime.Now;
        ws.Cells["B3"].Value = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, time.Kind).ToString();
        ws.Cells["A5:G5"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        ws.Cells["A5:G5"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 84, 84));
        ws.Cells["A4"].Value = "Completed Orders";
        ws.Cells["A5"].Value = "Shipped Date";
        ws.Cells["B5"].Value = "Order Id";
        ws.Cells["C5"].Value = "Email";
        ws.Cells["D5"].Value = "Phone#";
        ws.Cells["E5"].Value = "Income";
        ws.Cells["F5"].Value = "Expense";
        ws.Cells["G5"].Value = "Net Profit";
        var Row = 6;
        foreach (var detail in Reservations)
        {
            ws.Cells["A" + Row].Value = detail.ShippingDate.ToString();
            ws.Cells["B" + Row].Value = detail.Id;
            ws.Cells["C" + Row].Value = detail.ApplicationUser.Email;
            ws.Cells["D" + Row].Value = detail.Phone;
            ws.Cells["E" + Row].Value = detail.OrderTotal;
            ws.Cells["F" + Row].Value = detail.BaseTotal;
            ws.Cells["G" + Row].Value = detail.OrderTotal - detail.BaseTotal;
            Row++;
        }
        ws.Cells["A" + Row].Value = "Total:";
        ws.Cells["B" + Row].Value = "";
        ws.Cells["C" + Row].Value = "OverHead:";
        ws.Cells["D" + Row].Value = report.Overhead;
        ws.Cells["E" + Row].Value = report.GrossIncome;
        ws.Cells["F" + Row].Value = report.BaseCosts;
        ws.Cells["G" + Row].Value = report.NetIncome;

        ws.Cells[("A" + Row) + (":G" + Row)].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        ws.Cells[("A" + Row) + (":G" + Row)].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 84, 84));

        ws.Cells["A5:G" + (Row)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        ws.Cells["A5:G" + (Row)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        ws.Cells["A5:G" + (Row)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        ws.Cells["A5:G" + (Row)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        Row += 2;
        ws.Cells["A" + Row].Value = "Cancelled Orders";
        Row++;
        ws.Cells[("A" + Row) + (":E" + Row)].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        ws.Cells[("A" + Row) + (":E" + Row)].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 84, 84));
        ws.Cells["A" + Row].Value = "Cancel Date";
        ws.Cells["B" + Row].Value = "Order Id";
        ws.Cells["C" + Row].Value = "Email";
        ws.Cells["D" + Row].Value = "Phone#";
        ws.Cells["E" + Row].Value = "Cancel Reason";
        var Row2 = Row + 1;
        IEnumerable<ReservationHeader> cancelled = _unitOfWork.ReservationHeader.GetAll(u => u.OrderStatus == SD.StatusCancelled, includeProperties: ("ApplicationUser")).Where(u => u.CancelDate >= report.MinDate).Where(u => u.CancelDate <= report.MaxDate);
        foreach (var detail in cancelled)
        {
            ws.Cells["A" + Row2].Value = detail.CancelDate.ToString();
            ws.Cells["B" + Row2].Value = detail.Id;
            ws.Cells["C" + Row2].Value = detail.ApplicationUser.Email;
            ws.Cells["D" + Row2].Value = detail.Phone;
            ws.Cells["E" + Row2].Value = detail.CancelReason;
            Row2++;
        }
        ws.Cells["A" + Row + ":E" + (Row2-1)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        ws.Cells["A" + Row + ":E" + (Row2-1)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        ws.Cells["A" + Row + ":E" + (Row2-1)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        ws.Cells["A" + Row + ":E" + (Row2-1)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        ws.Cells["A:AZ"].AutoFitColumns();
        var fileDownloadName = "Sales_Report_" + report.Name + ".xlsx";
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

