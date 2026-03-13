using DB;
using DB.Repo;
using E_Shop.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace E_Shop.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "cart";
        private readonly IUnitOfWork unitOfWork;

        public CartController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var vm = new CartVM
            {
                Items = GetCartItems()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            if (quantity < 1)
                quantity = 1;

            var product = await unitOfWork.Products.GetByIdAsync(productId);
            if (product == null || !product.IsActive)
                return NotFound();

            var cart = GetCartItems();
            var existingItem = cart.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItemVM
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    SKU = product.SKU,
                    UnitPrice = product.Price,
                    Quantity = quantity
                });
            }

            SaveCartItems(cart);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var cart = GetCartItems();
            var itemToRemove = cart.FirstOrDefault(i => i.ProductId == productId);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                SaveCartItems(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Update(int productId, int quantity)
        {
            var cart = GetCartItems();
            var itemToUpdate = cart.FirstOrDefault(i => i.ProductId == productId);
            if (itemToUpdate == null)
                return RedirectToAction(nameof(Index));

            if (quantity <= 0)
            {
                cart.Remove(itemToUpdate);
            }
            else
            {
                itemToUpdate.Quantity = quantity;
            }

            SaveCartItems(cart);
            return RedirectToAction(nameof(Index));
        }

        public List<CartItemVM> GetCartItems()
        {
            var sessionCart = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrWhiteSpace(sessionCart))
                return new List<CartItemVM>();

            var cart = JsonSerializer.Deserialize<List<CartItemVM>>(sessionCart);
            return cart ?? new List<CartItemVM>();
        }

        private void SaveCartItems(List<CartItemVM> cartItems)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cartItems));
        }
    }
}
