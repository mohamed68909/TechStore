using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using SkiaSharp;
using TechStore.Entities.Models;
using TechStore.Entities.Repositories;
using TechStore.Entities.ViewModels;

namespace TechStore.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ImageUploadSettings _imageUploadSettings;

        public ProductService(IUnitOfWork unitOfWork, IOptions<ImageUploadSettings> imageUploadSettings)
        {
            _unitOfWork = unitOfWork;
            _imageUploadSettings = imageUploadSettings.Value;
        }

        // FIX 11: Includeword → includeWord
        public IEnumerable<Product> GetAllProducts() =>
            _unitOfWork.Product.GetAll(includeWord: "Category");

        // FIX 11: GetFirstorDefault → GetFirstOrDefault; Includeword → includeWord
        public Product? GetProductById(int id) =>
            _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id, includeWord: "Category");

        public IEnumerable<Product> GetProductsByCategoryId(int categoryId) =>
            _unitOfWork.Product.GetAll(x => x.CategoryId == categoryId, includeWord: "Category");

        public ProductVM GetProductForCreate() => new ProductVM
        {
            Product = new Product(),
            CategoryList = _unitOfWork.Category.GetAll()
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
        };

        public ProductVM GetProductForEdit(int id) => new ProductVM
        {
            // FIX 11: GetFirstorDefault → GetFirstOrDefault
            Product = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id),
            CategoryList = _unitOfWork.Category.GetAll()
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
        };

        public void AddProduct(ProductVM productVM, Stream? fileStream, string? fileName)
        {
            if (fileStream != null && !string.IsNullOrEmpty(fileName))
                productVM.Product.Img = ProcessAndSaveImage(fileStream, fileName);

            _unitOfWork.Product.Add(productVM.Product);
            _unitOfWork.Complete();
        }

        // FIX M-1: New image is processed BEFORE deleting the old one.
        // Previously: old image deleted first → if ProcessAndSaveImage threw, product was left with no image.
        public void UpdateProduct(ProductVM productVM, Stream? fileStream, string? fileName)
        {
            if (fileStream != null && !string.IsNullOrEmpty(fileName))
            {
                var newImagePath = ProcessAndSaveImage(fileStream, fileName); // Save new first
                DeleteOldImage(productVM.Product.Img);                        // Then delete old
                productVM.Product.Img = newImagePath;
            }
            _unitOfWork.Product.Update(productVM.Product);
            _unitOfWork.Complete();
        }

        public bool DeleteProduct(int id)
        {
            // FIX 11: GetFirstorDefault → GetFirstOrDefault
            var product = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);
            if (product == null) return false;
            DeleteOldImage(product.Img);
            _unitOfWork.Product.Remove(product);
            _unitOfWork.Complete();
            return true;
        }

        private string ProcessAndSaveImage(Stream fileStream, string fileName)
        {
            string uniqueFileName = Guid.NewGuid().ToString() + ".webp";
            string uploads = Path.Combine(_imageUploadSettings.WebRootPath, "Images", "Products");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
            string filePath = Path.Combine(uploads, uniqueFileName);

            using var codec = SKCodec.Create(fileStream);
            if (codec == null) throw new InvalidOperationException("Failed to decode image stream.");

            using var bitmap = SKBitmap.Decode(codec);
            if (bitmap == null) throw new InvalidOperationException("Failed to decode image bitmap.");

            int newWidth = bitmap.Width > 800 ? 800 : bitmap.Width;
            int newHeight = bitmap.Width > 800
                ? (int)(bitmap.Height * (800.0 / bitmap.Width))
                : bitmap.Height;

            var info = new SKImageInfo(newWidth, newHeight);
            using var resizedBitmap = bitmap.Resize(info, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));
            if (resizedBitmap == null) throw new InvalidOperationException("Failed to resize image.");

            using var image = SKImage.FromBitmap(resizedBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Webp, 75);
            if (data == null) throw new InvalidOperationException("Failed to encode image to WebP.");

            using var stream = File.OpenWrite(filePath);
            data.SaveTo(stream);

            return "/Images/Products/" + uniqueFileName;
        }

        private void DeleteOldImage(string? imgPath)
        {
            if (string.IsNullOrEmpty(imgPath)) return;
            var oldPath = Path.Combine(_imageUploadSettings.WebRootPath, imgPath.TrimStart('/'));
            if (File.Exists(oldPath)) File.Delete(oldPath);
        }
    }
}
