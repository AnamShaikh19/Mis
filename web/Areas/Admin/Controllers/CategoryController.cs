
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Utility;


namespace Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {

        private readonly IUnitOfWork _unitOfWrok;
        public CategoryController(IUnitOfWork unitOfWrok)
        {
            _unitOfWrok = unitOfWrok;
        }
        public IActionResult Index()
        {
            var objlist = _unitOfWrok.category.GetAll().ToList();
            return View(objlist);
        }
        
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if(category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name","The DisplayOrder cannot exactly match the Name."); 
            }
            if(ModelState.IsValid)
            {
                _unitOfWrok.category.Add(category);
                _unitOfWrok.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if(id==null || id == 0)
            {
                return NotFound();
            }
            Category? category = _unitOfWrok.category.Get(u => u.Id == id);
            if(category == null)
            {
                return NotFound();
            } 
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if(ModelState.IsValid)
            {
                _unitOfWrok.category.Update(category);
                _unitOfWrok.Save();
                TempData["success"] = "Category update successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        
        public IActionResult Delete(int? id)
        {
            if(id==null || id == 0)
            {
                return NotFound();
            }
            Category? category = _unitOfWrok.category.Get(u => u.Id == id);
            if(category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            Category? obj = _unitOfWrok.category.Get(u => u.Id == id);
            if(obj == null)
            {
                return  NotFound();
            }
            _unitOfWrok.category.Remove(obj);
            _unitOfWrok.Save();
            TempData["success"] = "Category delete successfully";
            return RedirectToAction("Index");
        }

    }
}