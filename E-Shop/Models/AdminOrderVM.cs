using Microsoft.AspNetCore.Mvc.Rendering;

namespace E_Shop.Models
{
    public class AdminOrderListItemVM
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int Status { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }
}
