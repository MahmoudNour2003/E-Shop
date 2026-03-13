using DB;
using System.Collections.Generic;

namespace E_Shop.Models
{
    public class ProductListVM
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public int? CategoryId { get; set; }
        public string Query { get; set; }
        public string Sort { get; set; } = "name";
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; }
    }
}
