using DB;
using System.Collections.Generic;

namespace E_Shop.Models
{
    public class ProductDetailsVM
    {
        public Product Product { get; set; }
        public Category Category { get; set; }
        public IEnumerable<Product> RelatedProducts { get; set; } = new List<Product>();
    }
}
