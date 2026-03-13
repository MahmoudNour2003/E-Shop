using System.ComponentModel.DataAnnotations;

namespace E_Shop.Models
{
    public class ProfileVM
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public List<ProfileAddressVM> Addresses { get; set; } = new();
        public ProfileAddressInputVM NewAddress { get; set; } = new();
        public ProfileAddressInputVM EditAddress { get; set; } = new();
        public int? EditAddressId { get; set; }
    }

    public class ProfileAddressVM
    {
        public int AddressId { get; set; }
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

    public class ProfileAddressInputVM
    {
        public int? AddressId { get; set; }

        [Required]
        public string Country { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string Street { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Zip Code")]
        public string Zip { get; set; } = string.Empty;

        [Display(Name = "Default address")]
        public bool IsDefault { get; set; }
    }
}
