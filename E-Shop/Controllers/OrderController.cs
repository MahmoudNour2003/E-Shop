using DB;
using DB.Repo;
using E_Shop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace E_Shop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private const string CartSessionKey = "cart";
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<APP_USER> _userManager;

        public OrderController(IUnitOfWork unitOfWork, UserManager<APP_USER> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var orders = (await _unitOfWork.Orders.FindAsync(o => o.UserId == userId))
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new CustomerOrderVM
                {
                    OrderId = o.OrderId,
                    OrderNumber = o.OrderNumber,
                    Status = o.Status,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount
                })
                .ToList();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var cartItems = GetCartItems();
            if (!cartItems.Any())
            {
                TempData["OrderMessage"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var addresses = (await _unitOfWork.Addresses.FindAsync(a => a.UserId == userId))
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.AddressId)
                .ToList();

            var vm = new CheckoutVM
            {
                SelectedAddressId = addresses.FirstOrDefault(a => a.IsDefault)?.AddressId ?? addresses.FirstOrDefault()?.AddressId,
                Addresses = addresses.Select(a => new CheckoutAddressVM
                {
                    AddressId = a.AddressId,
                    Country = a.Country,
                    City = a.City,
                    Street = a.Street,
                    Zip = a.Zip,
                    IsDefault = a.IsDefault
                }).ToList(),
                Items = cartItems.Select(i => new OrderItemVM
                {
                    ProductId = i.ProductId,
                    ProductName = i.Name,
                    SKU = i.SKU,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutVM model)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var cartItems = GetCartItems();
            var addresses = (await _unitOfWork.Addresses.FindAsync(a => a.UserId == userId)).ToList();

            if (!cartItems.Any())
            {
                TempData["OrderMessage"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            model.Addresses = addresses.Select(a => new CheckoutAddressVM
            {
                AddressId = a.AddressId,
                Country = a.Country,
                City = a.City,
                Street = a.Street,
                Zip = a.Zip,
                IsDefault = a.IsDefault
            }).ToList();

            model.Items = cartItems.Select(i => new OrderItemVM
            {
                ProductId = i.ProductId,
                ProductName = i.Name,
                SKU = i.SKU,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList();

            if (!model.SelectedAddressId.HasValue || !addresses.Any(a => a.AddressId == model.SelectedAddressId.Value))
            {
                ModelState.AddModelError(nameof(model.SelectedAddressId), "Please select a valid shipping address.");
            }

            var productIds = cartItems.Select(i => i.ProductId).Distinct().ToList();
            var products = (await _unitOfWork.Products.FindAsync(p => productIds.Contains(p.ProductId))).ToDictionary(p => p.ProductId);

            foreach (var item in cartItems)
            {
                if (!products.TryGetValue(item.ProductId, out var product) || !product.IsActive)
                {
                    ModelState.AddModelError(string.Empty, $"Product {item.Name} is no longer available.");
                    continue;
                }

                if (product.StockQuantity < item.Quantity)
                {
                    ModelState.AddModelError(string.Empty, $"Insufficient stock for {item.Name}.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    UserId = userId,
                    ShippingAddressId = model.SelectedAddressId!.Value,
                    OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
                    Status = 0,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = cartItems.Sum(i => i.LineTotal)
                };

                await _unitOfWork.Orders.AddAsync(order);

                var orderItems = new List<Order_Item>();
                foreach (var item in cartItems)
                {
                    var product = products[item.ProductId];
                    product.StockQuantity -= item.Quantity;
                    _unitOfWork.Products.Update(product);

                    orderItems.Add(new Order_Item
                    {
                        Order = order,
                        ProductId = item.ProductId,
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity,
                        LineTotal = item.LineTotal
                    });
                }

                await _unitOfWork.OrderItems.AddRangeAsync(orderItems);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                ModelState.AddModelError(string.Empty, "Unable to place order right now. Please try again.");
                return View(model);
            }

            SaveCartItems(new List<CartItemVM>());
            TempData["OrderMessage"] = "Order placed successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var order = await _unitOfWork.Orders.FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);
            if (order == null)
            {
                return NotFound();
            }

            var address = await _unitOfWork.Addresses.GetByIdAsync(order.ShippingAddressId);
            var orderItems = (await _unitOfWork.OrderItems.FindAsync(oi => oi.OrderId == id)).ToList();

            var productIds = orderItems.Select(oi => oi.ProductId).Distinct().ToList();
            var products = (await _unitOfWork.Products.FindAsync(p => productIds.Contains(p.ProductId))).ToDictionary(p => p.ProductId);

            var vm = new OrderDetailsVM
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                Status = order.Status,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                ShippingAddress = address == null ? null : new OrderShippingAddressVM
                {
                    Country = address.Country,
                    City = address.City,
                    Street = address.Street,
                    Zip = address.Zip
                },
                Items = orderItems.Select(oi => new OrderDetailItemVM
                {
                    OrderItemId = oi.OrderItemId,
                    ProductId = oi.ProductId,
                    ProductName = products.TryGetValue(oi.ProductId, out var p) ? p.Name : string.Empty,
                    SKU = products.TryGetValue(oi.ProductId, out var p2) ? p2.SKU : string.Empty,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity,
                    LineTotal = oi.LineTotal
                }).ToList()
            };

            return View(vm);
        }

        private List<CartItemVM> GetCartItems()
        {
            var sessionCart = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrWhiteSpace(sessionCart))
            {
                return new List<CartItemVM>();
            }

            return JsonSerializer.Deserialize<List<CartItemVM>>(sessionCart) ?? new List<CartItemVM>();
        }

        private void SaveCartItems(List<CartItemVM> cartItems)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cartItems));
        }
    }
}
