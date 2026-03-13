using System.ComponentModel.DataAnnotations;

namespace E_Shop.Models
{
    public class CheckoutVM
    {
        [Required(ErrorMessage = "Please select a shipping address.")]
        [Display(Name = "Shipping Address")]
        public int? SelectedAddressId { get; set; }

        [Display(Name = "Add order note")]
        [MaxLength(500)]
        public string? Notes { get; set; }

        public List<CheckoutAddressVM> Addresses { get; set; } = new();
        public List<OrderItemVM> Items { get; set; } = new();

        public decimal DiscountAmount { get; set; }
        public decimal ShippingAmount { get; set; }

        public int TotalItems => Items.Sum(i => i.Quantity);
        public int TotalLines => Items.Count;
        public decimal SubTotal => Items.Sum(i => i.LineTotal);
        public decimal Total => SubTotal - DiscountAmount + ShippingAmount;
        public bool IsEmpty => !Items.Any();
    }

    public class CheckoutAddressVM
    {
        public int AddressId { get; set; }
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
        public bool IsDefault { get; set; }

        public string FullAddress => $"{Street}, {City}, {Country} ({Zip})";
    }

    public class OrderItemVM
    {
        public int OrderId { get; set; }
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;
    }
}
