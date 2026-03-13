using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace E_Shop.Models
{
    public class AdminCategoryListItemVM
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ParentCategoryName { get; set; }
    }

    public class AdminCategoryFormVM
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Parent Category")]
        public int? ParentCategoryId { get; set; }

        public List<SelectListItem> ParentCategories { get; set; } = new();
    }
}
