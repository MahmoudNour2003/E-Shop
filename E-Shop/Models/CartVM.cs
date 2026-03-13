using System.Collections.Generic;
using System.Linq;

namespace E_Shop.Models
{
    public class CartVM
    {
        public List<CartItemVM> Items { get; set; } = new();
        public decimal DiscountAmount { get; set; }
        public decimal ShippingAmount { get; set; }

        public int TotalItems => Items.Sum(i => i.Quantity);
        public int TotalLines => Items.Count;
        public decimal SubTotal => Items.Sum(i => i.LineTotal);
        public decimal Total => SubTotal - DiscountAmount + ShippingAmount;
        public bool IsEmpty => !Items.Any();
    }

    public class CartItemVM
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;
    }
}
