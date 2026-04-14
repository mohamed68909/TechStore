
using Stripe.Checkout;
using TechStore.Entities.ViewModels;

namespace TechStore.Services.Interfaces
{
    public interface ICartService
    {
        ShoppingCartVM GetCartViewModel(string userId);
        ShoppingCartVM GetSummaryViewModel(string userId);
        Session CreateStripeSession(ShoppingCartVM vm, string userId, string domain);
        void ConfirmOrderPayment(int orderId, string sessionId);
        int IncrementItem(int cartId);
        int DecrementItem(int cartId);
        int RemoveItem(int cartId);
    }
}
