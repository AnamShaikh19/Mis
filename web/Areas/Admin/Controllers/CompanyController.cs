using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Models.ViewModels;

namespace Web.Areas.Admin.Controllers
{
     [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]

    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWrok;
        public CompanyController(IUnitOfWork unitOfWrok)
        {
            _unitOfWrok = unitOfWrok;
        }
        public IActionResult Index()
        {
            var objlist = _unitOfWrok.Company.GetAll().ToList();
            return View(objlist);
        }
        
        [HttpGet]
        public IActionResult Upsert(int? id)
        {
          
            if(id ==null || id == 0)
            {
                //create 
                return View(new Company());
            }
            else
            {
                //udpate
                Company companyobj = _unitOfWrok.Company.Get(u=>u.Id == id);
                return View(companyobj);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company Companyobj)
        {
            if(ModelState.IsValid)
            {
                
                if(Companyobj.Id == 0)
                {
                    _unitOfWrok.Company.Add(Companyobj);
                }
                else
                {
                    _unitOfWrok.Company.Update(Companyobj );
                }
                _unitOfWrok.Save();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index");
            }
            else{
               
                return View(Companyobj);
            }
        }
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if(id==null || id == 0)
            {
                return NotFound();
            }
            Company? Company = _unitOfWrok.Company.Get(u => u.Id == id);
            if(Company == null)
            {
                return NotFound();
            } 
            return View(Company);
        }

        [HttpPost]
        public IActionResult Edit(Company Company)
        {
            if(ModelState.IsValid)
            {
                _unitOfWrok.Company.Update(Company);
                _unitOfWrok.Save();
                TempData["success"] = "Comany update successfully";
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
        //     Company? Company = _unitOfWrok.Company.Get(u => u.Id == id);
        //     if(Company == null)
        //     {
        //         return NotFound();
        //     }
        //     return View(Company);
        // }
        // [HttpPost, ActionName("Delete")]
        // public IActionResult DeletePost(int? id)
        // {
        //     Company? obj = _unitOfWrok.Company.Get(u => u.Id == id);
        //     if(obj == null)
        //     {
        //         return  NotFound();
        //     }
        //     _unitOfWrok.Company.Remove(obj);
        //     _unitOfWrok.Save();
        //     TempData["success"] = "Category delete successfully";
        //     return RedirectToAction("Index");
        // }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> listOfCompany = _unitOfWrok.Company.GetAll().ToList();
            return Json(new {data = listOfCompany});    
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var CompanytoDelete = _unitOfWrok.Company.Get(p => p.Id == id);
            if(CompanytoDelete == null)
            {
                return Json(new {success=false,message="Error while deleting"});
            }
            
            _unitOfWrok.Company.Remove(CompanytoDelete);
            _unitOfWrok.Save();

            return Json(new { success=true, message="Delete Successful"});
        }

        #endregion API CALLS
    }
}