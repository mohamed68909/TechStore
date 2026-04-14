using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using TechStore.Entities.Models;
using TechStore.Entities.Repositories;
using TechStore.Entities.ViewModels;

namespace TechStore.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductService(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

        public IEnumerable<Product> GetAllProducts() => _unitOfWork.Product.GetAll(Includeword: "Category");

        public ProductVM GetProductForCreate() => new ProductVM
        {
            Product = new Product(),
            CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
        };

        public ProductVM GetProductForEdit(int id) => new ProductVM
        {
            Product = _unitOfWork.Product.GetFirstorDefault(x => x.Id == id),
            CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
        };

        public void AddProduct(ProductVM productVM, IFormFile file, string webRootPath)
        {
            if (file != null) productVM.Product.Img = ProcessAndSaveImage(file, webRootPath);
            _unitOfWork.Product.Add(productVM.Product);
            _unitOfWork.Complete();
        }

        public void UpdateProduct(ProductVM productVM, IFormFile file, string webRootPath)
        {
            if (file != null)
            {
                DeleteOldImage(productVM.Product.Img, webRootPath);
                productVM.Product.Img = ProcessAndSaveImage(file, webRootPath);
            }
            _unitOfWork.Product.Update(productVM.Product);
            _unitOfWork.Complete();
        }

        public bool DeleteProduct(int id, string webRootPath)
        {
            var product = _unitOfWork.Product.GetFirstorDefault(x => x.Id == id);
            if (product == null) return false;
            DeleteOldImage(product.Img, webRootPath);
            _unitOfWork.Product.Remove(product);
            _unitOfWork.Complete();
            return true;
        }

        private string ProcessAndSaveImage(IFormFile file, string webRootPath)
        {
            string fileName = Guid.NewGuid().ToString() + ".webp";
            string uploads = Path.Combine(webRootPath, "Images", "Products");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
            string filePath = Path.Combine(uploads, fileName);

            // ??? ???????? ????? ?????? ??? Image ????? ?? Confusion
            using (var image = SixLabors.ImageSharp.Image.Load(file.OpenReadStream()))
            {
                image.Mutate(x => x.Resize(new ResizeOptions { Size = new Size(800, 0), Mode = ResizeMode.Max }));
                image.Save(filePath, new SixLabors.ImageSharp.Formats.Webp.WebpEncoder { Quality = 75 });
            }
            return "/Images/Products/" + fileName;
        }

        private void DeleteOldImage(string imgPath, string webRootPath)
        {
            if (string.IsNullOrEmpty(imgPath)) return;
            var oldPath = Path.Combine(webRootPath, imgPath.TrimStart('/'));
            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
        }
    }
}
