
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechStore.Entities.ViewModels;
using TechStore.Entities.Repositories;
using TechStore.Services.Interfaces;
using TechStore.Utilities;

[Area("Customer")]
[Authorize]
public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IUnitOfWork _unitOfWork;

    public CartController(ICartService cartService, IUnitOfWork unitOfWork)
    {
        _cartService = cartService;
        _unitOfWork = unitOfWork;
    }

    private string GetUserId() => ((ClaimsIdentity)User.Identity!)?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    public IActionResult Index() => View(_cartService.GetCartViewModel(GetUserId()));

    public IActionResult Summary() => View(_cartService.GetSummaryViewModel(GetUserId()));

    [HttpPost]
    [ActionName("Summary")]
    [ValidateAntiForgeryToken]
    public IActionResult POSTSummary(ShoppingCartVM ShoppingCartVM)
    {
        try
        {
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var session = _cartService.CreateStripeSession(ShoppingCartVM, GetUserId(), domain);

            Response.Headers.Append("Location", session.Url);
            return new StatusCodeResult(303);
        }
        catch (Exception ex)
        {
            TempData["error"] = "Payment Service Error: " + ex.Message;
            return RedirectToAction("Summary");
        }
    }

    public IActionResult OrderConfirmation(int id)
    {
        var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id); // FIX 11
        if (orderHeader == null || orderHeader.ApplicationUserId != GetUserId())
        {
            return NotFound();
        }

        try
        {
            _cartService.ConfirmOrderPayment(id, GetUserId());
        }
        catch (Exception ex)
        {
            TempData["error"] = "Error confirming payment: " + ex.Message;
        }

        HttpContext.Session.Clear();
        return View(id);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Plus(int cartid)
    {
        _cartService.IncrementItem(cartid, GetUserId());
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Minus(int cartid)
    {
        var count = _cartService.DecrementItem(cartid, GetUserId());
        if (count == 0) UpdateSessionCount();
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int cartid)
    {
        var remainingCount = _cartService.RemoveItem(cartid, GetUserId());
        HttpContext.Session.SetInt32(SD.SessionKey, remainingCount);
        return RedirectToAction("Index");
    }

    private void UpdateSessionCount()
    {
        var count = _cartService.GetCartViewModel(GetUserId()).CartsList.Count();
        HttpContext.Session.SetInt32(SD.SessionKey, count);
    }
}
