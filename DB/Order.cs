using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DB
{
    public class Order
    {
        public int OrderId { get; set; }
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        public virtual APP_USER User { get; set; }
        [ForeignKey(nameof(Address))]
        public int ShippingAddressId { get; set; }
        public virtual Address Address { get; set; }
        [Required]
        public string OrderNumber { get; set; }
        public int Status { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<Order_Item> OrderItems { get; set; } = new List<Order_Item>();
    }
}
