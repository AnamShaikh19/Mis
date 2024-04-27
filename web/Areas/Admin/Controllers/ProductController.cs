using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Models.ViewModels;

namespace Web.Areas.Admin.Controllers
{
     [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]

    public class ProductController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnviroment;
        private readonly IUnitOfWork _unitOfWrok;
        public ProductController(IUnitOfWork unitOfWrok, IWebHostEnvironment webHostEnviroment)
        {
            _unitOfWrok = unitOfWrok;
            _webHostEnviroment = webHostEnviroment;
        }
        public IActionResult Index()
        {
            var objlist = _unitOfWrok.Product.GetAll(includeProperties:"Category").ToList();
            return View(objlist);
        }
        
        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM(){
                CategoryList = _unitOfWrok.category.GetAll().Select(u => new SelectListItem {
                    Text = u.Name,
                    Value=u.Id.ToString()
                }),
                Product = new Product()
            };
            if(id ==null || id == 0)
            {
                //create 
                return View(productVM);
            }
            else
            {
                //udpate
                productVM.Product = _unitOfWrok.Product.Get(u=>u.Id == id);
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if(ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnviroment.WebRootPath;
                if(file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if(!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        string oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath); 
                        }
                    }

                    using(var fileStream = new FileStream(Path.Combine(productPath,fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl =  @"\images\product\" + fileName;

                }
                if(productVM.Product.Id == 0)
                {
                    _unitOfWrok.Product.Add(productVM.Product);
                }
                else
                {
                    _unitOfWrok.Product.Update(productVM.Product);
                }
                _unitOfWrok.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            else{
                productVM.CategoryList = _unitOfWrok.category.GetAll().Select(u => new SelectListItem 
                {
                    Text = u.Name,
                    Value = u.Id.ToString()    
                });
                return View(productVM);
            }
        }
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if(id==null || id == 0)
            {
                return NotFound();
            }
            Product? product = _unitOfWrok.Product.Get(u => u.Id == id);
            if(product == null)
            {
                return NotFound();
            } 
            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            if(ModelState.IsValid)
            {
                _unitOfWrok.Product.Update(product);
                _unitOfWrok.Save();
                TempData["success"] = "Category update successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        
        // public IActionResult Delete(int? id)
        // {
        //     if(id==null || id == 0)
        //     {
        //         return NotFound();
        //     }
        //     Product? product = _unitOfWrok.Product.Get(u => u.Id == id);
        //     if(product == null)
        //     {
        //         return NotFound();
        //     }
        //     return View(product);
        // }
        // [HttpPost, ActionName("Delete")]
        // public IActionResult DeletePost(int? id)
        // {
        //     Product? obj = _unitOfWrok.Product.Get(u => u.Id == id);
        //     if(obj == null)
        //     {
        //         return  NotFound();
        //     }
        //     _unitOfWrok.Product.Remove(obj);
        //     _unitOfWrok.Save();
        //     TempData["success"] = "Category delete successfully";
        //     return RedirectToAction("Index");
        // }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> listOfProduct = _unitOfWrok.Product.GetAll(includeProperties:"Category").ToList();
            return Json(new {data = listOfProduct});    
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var producttoDelete = _unitOfWrok.Product.Get(p => p.Id == id);
            if(producttoDelete == null)
            {
                return Json(new {success=false,message="Error while deleting"});
            }
            if(!string.IsNullOrEmpty(producttoDelete.ImageUrl))
            {
                string oldImagePath = Path.Combine(_webHostEnviroment.WebRootPath, producttoDelete.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath); 
                        }
            }
            
            _unitOfWrok.Product.Remove(producttoDelete);
            _unitOfWrok.Save();

            return Json(new { success=true, message="Delete Successful"});
        }

        #endregion API CALLS
    }
}