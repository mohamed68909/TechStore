

using TechStore.Entities.Models;

namespace TechStore.Entities.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> CartsList { get; set; } = new List<ShoppingCart>();
        public OrderHeader OrderHeader { get; set; } = default!;
    }
}
