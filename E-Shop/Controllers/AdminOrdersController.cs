using DB.Repo;
using E_Shop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace E_Shop.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Orders")]
    public class AdminOrdersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminOrdersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var orders = (await _unitOfWork.Orders.GetAllAsync())
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            var userIds = orders.Select(o => o.UserId).Distinct().ToList();
            var users = (await _unitOfWork.Users.FindAsync(u => userIds.Contains(u.Id)))
                .ToDictionary(u => u.Id, u => u);

            var vm = orders.Select(o => new AdminOrderListItemVM
            {
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CustomerName = users.TryGetValue(o.UserId, out var user) ? user.FullName : "N/A",
                CustomerEmail = users.TryGetValue(o.UserId, out var user2) ? user2.Email ?? string.Empty : string.Empty,
                StatusOptions = GetStatusOptions(o.Status)
            }).ToList();

            return View(vm);
        }

        [HttpPost("UpdateStatus/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, int status)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
            {
                return RedirectToAction(nameof(Index));
            }

            order.Status = status;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.CompleteAsync();

            TempData["AdminMessage"] = "Order status updated.";
            return RedirectToAction(nameof(Index));
        }

        private static List<SelectListItem> GetStatusOptions(int selectedStatus)
        {
            var statuses = new[]
            {
                (Value: 0, Text: "Pending"),
                (Value: 1, Text: "Paid"),
                (Value: 2, Text: "Shipped"),
                (Value: 3, Text: "Delivered"),
                (Value: 4, Text: "Cancelled")
            };

            return statuses
                .Select(s => new SelectListItem(s.Text, s.Value.ToString(), s.Value == selectedStatus))
                .ToList();
        }
    }
}
