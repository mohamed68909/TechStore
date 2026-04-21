

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechStore.Entities.Models;

namespace TechStore.Entities.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; } = default!;
        [ValidateNever]
        public IEnumerable<SelectListItem> CategoryList { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
