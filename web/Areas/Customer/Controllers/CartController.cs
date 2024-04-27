using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Options;
using Models;
using Stripe.Checkout;
using System.Security.Claims;
using Utility;

namespace Web.Areas.Customer.Controllers
{
    [Area("customer")]
    [Authorize]
    public class CartController : Controller
    {
        
        private readonly IUnitOfWork _unitofwork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }
        public IActionResult Index()    
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userID = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                shoppingCartList = _unitofwork.ShoppingCart.GetAll(u => u.ApplicationUserId == userID, includeProperties: "Product"),
                OrderHeader =new()
            };
            foreach(var cart in ShoppingCartVM.shoppingCartList)
            {
                   cart.Price = GetPriceBasedOnQuntity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }
        public ActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userID = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                shoppingCartList = _unitofwork.ShoppingCart.GetAll(u => u.ApplicationUserId == userID, includeProperties: "Product"),
                OrderHeader = new()
            };
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitofwork.ApplicationUser.Get(u => u.Id == userID);
            ShoppingCartVM.OrderHeader.PhoneNumber =ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;


            foreach (var cart in ShoppingCartVM.shoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuntity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
		public ActionResult SummaryPost()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userID = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.shoppingCartList = _unitofwork.ShoppingCart.GetAll(u => u.ApplicationUserId
                == userID, includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userID;

            ApplicationUser applicationUser = _unitofwork.ApplicationUser.Get(u => u.Id == userID);

			foreach (var cart in ShoppingCartVM.shoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuntity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}
             if(applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // regular customer
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending; ;

			}
            else
            {
                // regular company user
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus =SD.StatusApproved; 
            }
             _unitofwork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitofwork.Save();
                foreach(var cart in ShoppingCartVM.shoppingCartList)
                {
                    OrderDetail orderDetail = new()
                    {
				    	ProductId = cart.ProductId,
                        OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                        Price = cart.Price,
                        Count = cart.Count
			         };
                    _unitofwork.OrderDetail.Add(orderDetail);
                    _unitofwork.Save();
                }
                if(applicationUser.CompanyId.GetValueOrDefault() == 0)
                {
                    //it is regular customer account and need to capture payment
                    //stripe logic

                    var domain = "https://localhost:7095/";

                    var options = new Stripe.Checkout.SessionCreateOptions
                    {
                        SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                        CancelUrl = domain + "customer/cart/index",
                        LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),


                        Mode = "payment",
                    };
                    foreach(var item in ShoppingCartVM.shoppingCartList)
                    {
                        var sessionLineItem = new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(item.Price * 100),
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = item.Product.Title
                                }
                            },
                            Quantity = item.Count
                        };
                        options.LineItems.Add(sessionLineItem);
                    }
                    var service = new SessionService();
                    Session session = service.Create(options);
                    _unitofwork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                    _unitofwork.Save();
                    Response.Headers.Add("Location", session.Url);
                    return new StatusCodeResult(303);
            }
            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
		}
        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitofwork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            if(orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                //this is order by customer
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitofwork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    _unitofwork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitofwork.Save();
                }

                HttpContext.Session.Clear();

            }
            List<ShoppingCart> shoppingCarts = _unitofwork.ShoppingCart.GetAll
                (u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitofwork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitofwork.Save(); 

            return View(id);
        }


		public ActionResult Plus(int cartId)
        {
            var cartFromDb = _unitofwork.ShoppingCart.Get(u =>u.Id == cartId);
            cartFromDb.Count += 1;
            _unitofwork.ShoppingCart.Update(cartFromDb);
            _unitofwork.Save();
            return RedirectToAction(nameof(Index));

        }
        public ActionResult Minus(int cartId)
        {
            var cartFromDb = _unitofwork.ShoppingCart.Get(u => u.Id == cartId);
            if (cartFromDb.Count <= 1)
            {
                //remove from cart
                _unitofwork.ShoppingCart.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitofwork.ShoppingCart.Update(cartFromDb);
            }
            _unitofwork.Save();
            return RedirectToAction(nameof(Index));

        }
        public ActionResult Remove(int cartId)
        {
            var cartFromDb = _unitofwork.ShoppingCart.Get(u => u.Id == cartId);
           
                //remove from cart
                _unitofwork.ShoppingCart.Remove(cartFromDb);
            
            
            _unitofwork.Save();
            return RedirectToAction(nameof(Index));

        }
        public double GetPriceBasedOnQuntity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }

            }
        }
    }
}

