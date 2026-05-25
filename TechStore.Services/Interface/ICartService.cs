
using Stripe.Checkout;
using TechStore.Entities.ViewModels;

namespace TechStore.Services.Interfaces
{
    public interface ICartService
    {
        ShoppingCartVM GetCartViewModel(string userId);
        ShoppingCartVM GetSummaryViewModel(string userId);

        // FIX 3: Async version of CreateStripeSession for proper transaction support
        Task<Session> CreateStripeSessionAsync(ShoppingCartVM vm, string userId, string domain,
            string? successUrl = null, string? cancelUrl = null);

        // Kept for backward compat with MVC CartController (sync)
        Session CreateStripeSession(ShoppingCartVM vm, string userId, string domain,
            string? successUrl = null, string? cancelUrl = null);

        void ConfirmOrderPayment(int orderId, string userId);

        int IncrementItem(int cartId, string userId);
        int DecrementItem(int cartId, string userId);
        int RemoveItem(int cartId, string userId);

        // FIX 6: AddToCart moved to service layer with product-existence validation
        bool AddToCart(string userId, int productId, int count);
    }
}
