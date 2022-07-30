﻿using Microsoft.AspNetCore.Mvc;
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
public class ReservationReportController : Controller
{
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<IdentityUser> _userManager;

    public ReservationReportController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, IEmailSender emailSender)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _emailSender = emailSender;
    }


    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Reservations(int id)
    {
        ReservationReport Reservations = _unitOfWork.ReservationReport.GetFirstOrDefault(u => u.Id == id);
        return View(Reservations);
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
        ReservationReport Reservation = new();

        if (id == null || id == 0)
        {
            //create
            return View(Reservation);
        }
        else
        {
            //update
            Reservation = _unitOfWork.ReservationReport.GetFirstOrDefault(u => u.Id == id);
            return View(Reservation);
        }

    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(ReservationReport Reservation)
    {
        if (ModelState.IsValid)
        {
            if (Reservation.Id == 0)
            {
                IEnumerable<ReservationHeader> ReservationItem = _unitOfWork.ReservationHeader.GetAll(u => u.OrderStatus == Reservation.ReservationStatus).Where(u => u.OrderDate >= Reservation.MinDate).Where(u => u.OrderDate <= Reservation.MaxDate);

                Reservation.ReservationAmount = ReservationItem.Count();
                DateTime today = DateTime.Now;
                Reservation.GenerationDate = new DateTime(today.Year, today.Month, today.Day, today.Hour, today.Minute, today.Second, today.Kind);

                foreach (ReservationHeader item in ReservationItem) {
                    ReportDetail detail = new ReportDetail
                    {
                        HeaderId = item.Id,
                        ReportId = Reservation.Id,
                        ReportType = "Reservation",
                        reservationHeader = item
                    };
                    _unitOfWork.ReportDetail.Add(detail);
                }
                
                _unitOfWork.ReservationReport.Add(Reservation);
                TempData["Success"] = "Report Generated Successfully";
            }
            else
            {
                IEnumerable<ReservationHeader> ReservationItem = _unitOfWork.ReservationHeader.GetAll(u => u.OrderStatus == Reservation.ReservationStatus).Where(u => u.ShippingDate >= Reservation.MinDate).Where(u => u.ShippingDate <= Reservation.MaxDate);

                Reservation.ReservationAmount = ReservationItem.Count();
                DateTime today = DateTime.Now;
                Reservation.GenerationDate = new DateTime(today.Year, today.Month, today.Day, today.Hour, today.Minute, today.Second, today.Kind);

                _unitOfWork.ReportDetail.RemoveRange(_unitOfWork.ReportDetail.GetAll(u => u.ReportType == "Reservation" && u.ReportId == Reservation.Id));
                foreach (ReservationHeader item in ReservationItem)
                {
                    ReportDetail detail = new ReportDetail
                    {
                        HeaderId = item.Id,
                        ReportId = Reservation.Id,
                        ReportType = "Reservation",
                        reservationHeader = item
                    };
                    _unitOfWork.ReportDetail.Add(detail);
                }

                _unitOfWork.ReservationReport.Update(Reservation);
                TempData["Success"] = "Report Generated Successfully";
            }

            _unitOfWork.Save();

            return RedirectToAction("Index");
        }
        return View(Reservation);
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
        var ReservationList = _unitOfWork.ReservationReport.GetAll();
        return Json(new { data = ReservationList });
    }

    public IActionResult GetAll2(int id)
    {
        IEnumerable<ReportDetail> Reservations = _unitOfWork.ReportDetail.GetAll(u => u.ReportType == "Reservation" && u.ReportId == id, includeProperties:"reservationHeader,reservationHeader.ApplicationUser");

        foreach (var head in Reservations)
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

        return Json(new { data = Reservations });
    }

    //POST
    [HttpDelete]
    public IActionResult Delete(int? id)
    {
        var obj = _unitOfWork.ReservationReport.GetFirstOrDefault(u => u.Id == id);
        if (obj == null)
        {
            return Json(new { success = false, message = "Error while deleting" });
        }
        _unitOfWork.ReportDetail.RemoveRange(_unitOfWork.ReportDetail.GetAll(u => u.ReportId == id && u.ReportType=="Reservation"));
        _unitOfWork.ReservationReport.Remove(obj);
        _unitOfWork.Save();
        return Json(new { success = true, message = "Report Deleted Successfully" });
    }

    #endregion
}

