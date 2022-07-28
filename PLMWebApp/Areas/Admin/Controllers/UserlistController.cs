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
[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Logistics + "," + SD.Role_Sales)]
public class UserlistController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserlistController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    //GET
    public IActionResult Update(string email)
    {
        UserVM user = new();
        user.applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Email == email);
        user.RolesList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
        {
            Text = i,
            Value = i
        });
        return View(user);
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UserVM userVM)
    {
        //if (ModelState.IsValid)
        //{
        ApplicationUser user = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.Email == userVM.applicationUser.Email);

        if (user.RoleName != userVM.applicationUser.RoleName)
        {
            var realuser = _userManager.FindByIdAsync(user.Id).Result;
            await _userManager.RemoveFromRoleAsync(realuser, user.RoleName);
            await _userManager.AddToRoleAsync(realuser, userVM.applicationUser.RoleName);
            user.RoleName = userVM.applicationUser.RoleName;
        }
        if (userVM.applicationUser.Warnings == 1)
        {
            user.Warnings = 1;
            user.Warning1 = userVM.applicationUser.Warning1;
            user.Warning2 = "";
        }
        else if (userVM.applicationUser.Warnings == 2)
        {
            user.Warnings = 2;
            user.Warning1 = userVM.applicationUser.Warning1;
            user.Warning2 = userVM.applicationUser.Warning2;
        }
        else
        {
            user.Warnings = 0;
            user.Warning1 = "";
            user.Warning2 = "";
        }
        _unitOfWork.Save();

        return RedirectToAction("Index");
        //}
        //return View(userVM);
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
        var userList = _unitOfWork.ApplicationUser.GetAll();
        return Json(new { data = userList });
    }

    ////POST
    //[HttpDelete]
    //public IActionResult Delete(int? id)
    //{
    //    var obj = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
    //    if (obj == null)
    //    {
    //        return Json(new { success = false, message = "Error while deactivating" });
    //    }

    //    //var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
    //    //if (System.IO.File.Exists(oldImagePath))
    //    //{
    //    //    System.IO.File.Delete(oldImagePath);
    //    //}
    //    obj.IsActive = false;
    //    obj.Inactivity = DateTime.Now;
    //    _unitOfWork.Product.Update(obj);
    //    _unitOfWork.Save();
    //    return Json(new { success = true, message = "Product Deactivated Successfully" });
    //    //return RedirectToAction("Index");

    //}

    //public IActionResult Activate(int? id)
    //{
    //    var obj = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id);
    //    if (obj == null)
    //    {
    //        return Json(new { success = false, message = "Error while Reactivating" });
    //    }

    //    //var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
    //    //if (System.IO.File.Exists(oldImagePath))
    //    //{
    //    //    System.IO.File.Delete(oldImagePath);
    //    //}
    //    obj.IsActive = true;
    //    _unitOfWork.Product.Update(obj);
    //    _unitOfWork.Save();
    //    return Json(new { success = true, message = "Product Reactivated Successfully" });
    //    //return RedirectToAction("Index");

    //}

    #endregion
}

