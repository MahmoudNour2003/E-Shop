using DB;
using DB.Repo;
using E_Shop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace E_Shop.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Products")]
    public class AdminProductsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminProductsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var products = (await _unitOfWork.Products.GetAllAsync()).ToList();
            var categories = (await _unitOfWork.Categories.GetAllAsync()).ToDictionary(c => c.CategoryId, c => c.Name);

            var vm = products
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new AdminProductListItemVM
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    SKU = p.SKU,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    IsActive = p.IsActive,
                    CategoryName = categories.TryGetValue(p.CategoryId, out var categoryName) ? categoryName : "N/A"
                })
                .ToList();

            return View(vm);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new AdminProductFormVM();
            await PopulateCategories(vm);
            return View(vm);
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminProductFormVM model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategories(model);
                return View(model);
            }

            var skuExists = await _unitOfWork.Products.ExistsAsync(p => p.SKU == model.SKU.Trim());
            if (skuExists)
            {
                ModelState.AddModelError(nameof(model.SKU), "SKU already exists.");
                await PopulateCategories(model);
                return View(model);
            }

            await _unitOfWork.Products.AddAsync(new Product
            {
                Name = model.Name.Trim(),
                SKU = model.SKU.Trim(),
                CategoryId = model.CategoryId,
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            });

            await _unitOfWork.CompleteAsync();
            TempData["AdminMessage"] = "Product created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var vm = new AdminProductFormVM
            {
                ProductId = product.ProductId,
                Name = product.Name,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive
            };

            await PopulateCategories(vm);
            return View(vm);
        }

        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminProductFormVM model)
        {
            if (id != model.ProductId)
            {
                return BadRequest();
            }

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateCategories(model);
                return View(model);
            }

            var skuExists = await _unitOfWork.Products.ExistsAsync(p => p.ProductId != id && p.SKU == model.SKU.Trim());
            if (skuExists)
            {
                ModelState.AddModelError(nameof(model.SKU), "SKU already exists.");
                await PopulateCategories(model);
                return View(model);
            }

            product.Name = model.Name.Trim();
            product.SKU = model.SKU.Trim();
            product.CategoryId = model.CategoryId;
            product.Price = model.Price;
            product.StockQuantity = model.StockQuantity;
            product.IsActive = model.IsActive;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.CompleteAsync();

            TempData["AdminMessage"] = "Product updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var hasOrderItems = await _unitOfWork.OrderItems.ExistsAsync(oi => oi.ProductId == id);
            if (hasOrderItems)
            {
                TempData["AdminError"] = "Cannot delete product that has order history.";
                return RedirectToAction(nameof(Index));
            }

            _unitOfWork.Products.Remove(product);
            await _unitOfWork.CompleteAsync();
            TempData["AdminMessage"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCategories(AdminProductFormVM model)
        {
            var categories = (await _unitOfWork.Categories.GetAllAsync())
                .OrderBy(c => c.Name)
                .ToList();

            model.Categories = categories
                .Select(c => new SelectListItem(c.Name, c.CategoryId.ToString()))
                .ToList();
        }
    }
}
