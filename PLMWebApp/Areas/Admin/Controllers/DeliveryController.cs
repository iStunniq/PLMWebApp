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
public class DeliveryController : Controller
{
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<IdentityUser> _userManager;

    public DeliveryController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, IEmailSender emailSender)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _emailSender = emailSender;
    }


    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Deliveries(int id)
    {
        DeliveryReport Deliveries = _unitOfWork.DeliveryReport.GetFirstOrDefault(u => u.Id == id);
        return View(Deliveries);
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
        DeliveryReport Delivery = new();

        if (id == null || id == 0)
        {
            //create
            return View(Delivery);
        }
        else
        {
            //update
            Delivery = _unitOfWork.DeliveryReport.GetFirstOrDefault(u => u.Id == id);
            return View(Delivery);
        }

    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(DeliveryReport Delivery)
    {
        if (ModelState.IsValid)
        {
            if (Delivery.Id == 0)
            {
                IEnumerable<ReservationHeader> DeliveryItem = _unitOfWork.ReservationHeader.GetAll(u => u.OrderStatus == SD.StatusShipped).Where(u => u.ShippingDate >= Delivery.MinDate).Where(u => u.ShippingDate <= Delivery.MaxDate);

                Delivery.ReservationAmount = DeliveryItem.Count();
                DateTime today = DateTime.Now;
                Delivery.GenerationDate = new DateTime(today.Year, today.Month, today.Day, today.Hour, today.Minute, today.Second, today.Kind);
                _unitOfWork.DeliveryReport.Add(Delivery);
                foreach (ReservationHeader item in DeliveryItem)
                {
                    ReportDetail detail = new ReportDetail
                    {
                        HeaderId = item.Id,
                        ReportId = Delivery.Id,
                        ReportType = "Delivery",
                        reservationHeader = item
                    };
                    _unitOfWork.ReportDetail.Add(detail);
                }

                TempData["Success"] = "Report Generated Successfully";
            }
            else
            {
                IEnumerable<ReservationHeader> DeliveryItem = _unitOfWork.ReservationHeader.GetAll(u => u.OrderStatus == SD.StatusShipped).Where(u => u.ShippingDate >= Delivery.MinDate).Where(u => u.ShippingDate <= Delivery.MaxDate);

                Delivery.ReservationAmount = DeliveryItem.Count();
                DateTime today = DateTime.Now;
                Delivery.GenerationDate = new DateTime(today.Year, today.Month, today.Day, today.Hour, today.Minute, today.Second, today.Kind);
                _unitOfWork.DeliveryReport.Update(Delivery);
                _unitOfWork.ReportDetail.RemoveRange(_unitOfWork.ReportDetail.GetAll(u => u.ReportType == "Delivery" && u.ReportId == Delivery.Id));
                foreach (ReservationHeader item in DeliveryItem)
                {
                    ReportDetail detail = new ReportDetail
                    {
                        HeaderId = item.Id,
                        ReportId = Delivery.Id,
                        ReportType = "Delivery",
                        reservationHeader = item
                    };
                    _unitOfWork.ReportDetail.Add(detail);
                }

                TempData["Success"] = "Report Generated Successfully";
            }

            _unitOfWork.Save();

            return RedirectToAction("Index");
        }
        return View(Delivery);
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
        var DeliveryList = _unitOfWork.DeliveryReport.GetAll();
        return Json(new { data = DeliveryList });
    }

    public IActionResult GetAll2(int id)
    {
        IEnumerable<ReportDetail> Deliveries = _unitOfWork.ReportDetail.GetAll(u => u.ReportType == "Delivery" && u.ReportId == id, includeProperties: "reservationHeader,reservationHeader.ApplicationUser");

        foreach (var head in Deliveries)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ApplicationUser user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Id == claim.Value);
            ReservationViewed view = _unitOfWork.ReservationViewed.GetFirstOrDefault(u => u.OrderId == head.reservationHeader.Id && u.AlertEmail == user.Email);
            if (view != null)
            {
                head.reservationHeader.Viewed = false;
            }
            else
            {
                head.reservationHeader.Viewed = true;
            }
            _unitOfWork.Save();
        };

        return Json(new { data = Deliveries });
    }

    public IActionResult Excel(int id)
    {
        DeliveryReport report = _unitOfWork.DeliveryReport.GetFirstOrDefault(u => u.Id == id);
        IEnumerable<ReportDetail> Reservations = _unitOfWork.ReportDetail.GetAll(u => u.ReportType == "Delivery" && u.ReportId == id, includeProperties: "reservationHeader,reservationHeader.ApplicationUser");
        Reservations.OrderBy(u => u.reservationHeader.ShippingDate);
        ExcelPackage pack = new ExcelPackage();
        ExcelWorksheet ws = pack.Workbook.Worksheets.Add("Report");

        ws.Cells["A1"].Value = "Delivery Report";
        ws.Cells["B1"].Value = report.Name;
        ws.Cells["C1"].Value = "Amount";
        ws.Cells["D1"].Value = report.ReservationAmount;
        ws.Cells["A2"].Value = "Report Generated";
        ws.Cells["B2"].Value = report.GenerationDate.ToString();
        ws.Cells["A3"].Value = "Excel Generated";
        var time = DateTime.Now;
        ws.Cells["B3"].Value = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, time.Kind).ToString();
        ws.Cells["A5:H5"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        ws.Cells["A5:H5"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 84, 84));

        ws.Cells["A5"].Value = "Ship Date";
        ws.Cells["B5"].Value = "Order Id";
        ws.Cells["C5"].Value = "Email";
        ws.Cells["D5"].Value = "Phone#";
        ws.Cells["E5"].Value = "Total";
        ws.Cells["F5"].Value = "Pay Type";
        ws.Cells["G5"].Value = "Courier";
        ws.Cells["H5"].Value = "Status";
        var Row = 6;
        foreach (var detail in Reservations)
        {
            ws.Cells["A" + Row].Value = detail.reservationHeader.ShippingDate.ToString();
            ws.Cells["B" + Row].Value = detail.reservationHeader.Id;
            ws.Cells["C" + Row].Value = detail.reservationHeader.ApplicationUser.Email;
            ws.Cells["D" + Row].Value = detail.reservationHeader.Phone;
            ws.Cells["E" + Row].Value = detail.reservationHeader.OrderTotal;
            
            if (detail.reservationHeader.COD) { ws.Cells["F" + Row].Value = "COD"; }
            else { ws.Cells["F" + Row].Value = "GCash"; }
            
            ws.Cells["G" + Row].Value = detail.reservationHeader.Carrier;
            ws.Cells["H" + Row].Value = detail.reservationHeader.OrderStatus;
            Row++;
        }
        ws.Cells["A:AZ"].AutoFitColumns();
        ws.Cells["A5:H" + (Row - 1)].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        ws.Cells["A5:H" + (Row - 1)].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        ws.Cells["A5:H" + (Row - 1)].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        ws.Cells["A5:H" + (Row - 1)].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

        var fileDownloadName = "Delivery_Report_" + report.Name + ".xlsx";
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
        var obj = _unitOfWork.DeliveryReport.GetFirstOrDefault(u => u.Id == id);
        if (obj == null)
        {
            return Json(new { success = false, message = "Error while deleting" });
        }
        _unitOfWork.ReportDetail.RemoveRange(_unitOfWork.ReportDetail.GetAll(u => u.ReportId == id && u.ReportType == "Delivery"));
        _unitOfWork.DeliveryReport.Remove(obj);
        _unitOfWork.Save();
        return Json(new { success = true, message = "Report Deleted Successfully" });
    }

    #endregion
}

