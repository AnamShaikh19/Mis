using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;


namespace Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ShopController : Controller
    {
        private readonly ILogger<ShopController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public ShopController(ILogger<ShopController> logger , IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> porductList = _unitOfWork.Product.GetAll(includeProperties: "Category");

            return View(porductList);
        }
        public IActionResult Details(int productId) {


            ShoppingCart Cart = new() {
                Product = _unitOfWork.Product.Get(p => p.Id == productId, includeProperties: "Category"),
                Count = 1,
               ProductId = productId
            };
            return View(Cart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userID = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userID;
            
            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userID &&
            u.ProductId == shoppingCart.ProductId); 
            if(cartFromDb != null)
            {
                //Shopping cart Exist
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            else
            {
                // Add cart record
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                

            }
            TempData["Success"] = "Cart Update Successfully";
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
            
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}