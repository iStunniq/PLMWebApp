﻿using Microsoft.AspNetCore.Mvc;
using PLM.DataAccess;
using PLM.DataAccess.Repository.IRepository;
using PLM.Models;

namespace PLMWebApp.Controllers;
[Area("Admin")]

    public class BrandController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public BrandController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
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

            _unitOfWork.Brand.Remove(obj);
            _unitOfWork.Save();
            TempData["Success"] = "Brand Deleted Successfully";
            return RedirectToAction("Index");

        }
    }

