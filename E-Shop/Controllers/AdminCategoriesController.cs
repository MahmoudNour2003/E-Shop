using DB;
using DB.Repo;
using E_Shop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace E_Shop.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Categories")]
    public class AdminCategoriesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminCategoriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var categories = (await _unitOfWork.Categories.GetAllAsync()).ToList();
            var vm = categories
                .OrderBy(c => c.Name)
                .Select(c => new AdminCategoryListItemVM
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    ParentCategoryName = categories.FirstOrDefault(p => p.CategoryId == c.ParentCategoryId)?.Name
                })
                .ToList();

            return View(vm);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = new AdminCategoryFormVM();
            await PopulateParentCategories(vm);
            return View(vm);
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminCategoryFormVM model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateParentCategories(model);
                return View(model);
            }

            var exists = await _unitOfWork.Categories.ExistsAsync(c => c.Name == model.Name.Trim());
            if (exists)
            {
                ModelState.AddModelError(nameof(model.Name), "Category name already exists.");
                await PopulateParentCategories(model);
                return View(model);
            }

            await _unitOfWork.Categories.AddAsync(new Category
            {
                Name = model.Name.Trim(),
                ParentCategoryId = model.ParentCategoryId
            });

            await _unitOfWork.CompleteAsync();
            TempData["AdminMessage"] = "Category created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var vm = new AdminCategoryFormVM
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                ParentCategoryId = category.ParentCategoryId
            };

            await PopulateParentCategories(vm, category.CategoryId);
            return View(vm);
        }

        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminCategoryFormVM model)
        {
            if (id != model.CategoryId)
            {
                return BadRequest();
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateParentCategories(model, model.CategoryId);
                return View(model);
            }

            var exists = await _unitOfWork.Categories.ExistsAsync(c => c.CategoryId != id && c.Name == model.Name.Trim());
            if (exists)
            {
                ModelState.AddModelError(nameof(model.Name), "Category name already exists.");
                await PopulateParentCategories(model, model.CategoryId);
                return View(model);
            }

            category.Name = model.Name.Trim();
            category.ParentCategoryId = model.ParentCategoryId;
            _unitOfWork.Categories.Update(category);
            await _unitOfWork.CompleteAsync();

            TempData["AdminMessage"] = "Category updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var hasProducts = await _unitOfWork.Products.ExistsAsync(p => p.CategoryId == id);
            var hasChildren = await _unitOfWork.Categories.ExistsAsync(c => c.ParentCategoryId == id);
            if (hasProducts || hasChildren)
            {
                TempData["AdminError"] = "Cannot delete category with related products or child categories.";
                return RedirectToAction(nameof(Index));
            }

            _unitOfWork.Categories.Remove(category);
            await _unitOfWork.CompleteAsync();
            TempData["AdminMessage"] = "Category deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateParentCategories(AdminCategoryFormVM model, int? currentCategoryId = null)
        {
            var categories = (await _unitOfWork.Categories.GetAllAsync())
                .Where(c => !currentCategoryId.HasValue || c.CategoryId != currentCategoryId.Value)
                .OrderBy(c => c.Name)
                .ToList();

            model.ParentCategories = categories
                .Select(c => new SelectListItem(c.Name, c.CategoryId.ToString()))
                .ToList();
        }
    }
}
