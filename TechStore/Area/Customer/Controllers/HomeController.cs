
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechStore.Entities.Models;
using TechStore.Services.Interfaces;
using TechStore.Utilities;

namespace TechStore.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        public IActionResult Index(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 8;

            var products = _homeService.GetProductsPaged(pageNumber, pageSize);
            return View(products);
        }

        public IActionResult Details(int ProductId)
        {
            var cartObj = _homeService.GetProductDetails(ProductId);
            return View(cartObj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            // ??????? ???? ???????? (UserId)
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;

            // ????? ??????? ??? ??????
            int cartCount = _homeService.AddOrUpdateCart(shoppingCart);

            // ??? ???? ?????? ???? ???? ?? 0? ???? ??? ????? ???? ???? (New Row) ????? ??? Session
            if (cartCount > 0)
            {
                HttpContext.Session.SetInt32(SD.SessionKey, cartCount);
            }

            return RedirectToAction("Index");
        }
    }
}
