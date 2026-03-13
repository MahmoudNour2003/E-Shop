using DB;
using DB.Repo;
using E_Shop.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace E_Shop.Controllers
{
    public class CatalogController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private const int PageSize = 12;

        public CatalogController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(int? categoryId, string q, string sort = "name", int page = 1)
        {
            page = Math.Max(1, page);
            q = q?.Trim();

            var products = await unitOfWork.Products.FindAsync(p =>
                p.IsActive &&
                (!categoryId.HasValue || p.CategoryId == categoryId.Value) &&
                (string.IsNullOrEmpty(q) || p.Name.Contains(q) || p.SKU.Contains(q)));

            var query = products.AsQueryable();
            query = sort?.ToLower() switch
            {
                "price_desc" => query.OrderByDescending(p => p.Price),
                "price_asc" => query.OrderBy(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Name)
            };

            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            var items = query.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            var vm = new ProductListVM
            {
                Products = items,
                Categories = await unitOfWork.Categories.GetAllAsync(),
                CategoryId = categoryId,
                Query = q,
                Sort = sort,
                Page = page,
                TotalPages = totalPages
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await unitOfWork.Products.GetByIdAsync(id);
            if (product == null || !product.IsActive)
                return NotFound();

            var vm = new ProductDetailsVM
            {
                Product = product,
                Category = await unitOfWork.Categories.GetByIdAsync(product.CategoryId),
                RelatedProducts = (await unitOfWork.Products.FindAsync(p =>
                    p.IsActive &&
                    p.CategoryId == product.CategoryId &&
                    p.ProductId != product.ProductId))
                    .Take(4)
                    .ToList()
            };

            return View(vm);
        }
    }
}
