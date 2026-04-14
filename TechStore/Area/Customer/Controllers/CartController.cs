
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechStore.Entities.ViewModels;
using TechStore.Services.Interfaces;
using TechStore.Utilities;

[Area("Customer")]
[Authorize]
public class CartController : Controller
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private string GetUserId() => ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.NameIdentifier).Value;

    public IActionResult Index() => View(_cartService.GetCartViewModel(GetUserId()));

    public IActionResult Summary() => View(_cartService.GetSummaryViewModel(GetUserId()));

    [HttpPost]
    [ActionName("Summary")]
    [ValidateAntiForgeryToken]
    public IActionResult POSTSummary(ShoppingCartVM ShoppingCartVM)
    {
        var domain = Request.Scheme + "://" + Request.Host.Value + "/";
        var session = _cartService.CreateStripeSession(ShoppingCartVM, GetUserId(), domain);

        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);
    }

    public IActionResult OrderConfirmation(int id)
    {
        var order = _cartService.GetSummaryViewModel(GetUserId()).OrderHeader; // ????? SessionId
        var sessionId = _cartService.GetCartViewModel(GetUserId()).OrderHeader.SessionId; // ???? ????? ?? ??? DB

        // ??????: ???? ????? ??? OrderHeader ?????? ?? ??????
        _cartService.ConfirmOrderPayment(id, order.SessionId);

        HttpContext.Session.Clear();
        return View(id);
    }

    public IActionResult Plus(int cartid)
    {
        _cartService.IncrementItem(cartid);
        return RedirectToAction("Index");
    }

    public IActionResult Minus(int cartid)
    {
        var count = _cartService.DecrementItem(cartid);
        if (count == 0) UpdateSessionCount();
        return RedirectToAction("Index");
    }

    public IActionResult Remove(int cartid)
    {
        var remainingCount = _cartService.RemoveItem(cartid);
        HttpContext.Session.SetInt32(SD.SessionKey, remainingCount);
        return RedirectToAction("Index");
    }

    private void UpdateSessionCount()
    {
        var count = _cartService.GetCartViewModel(GetUserId()).CartsList.Count();
        HttpContext.Session.SetInt32(SD.SessionKey, count);
    }
}
