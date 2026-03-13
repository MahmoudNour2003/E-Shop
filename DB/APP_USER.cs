using Microsoft.AspNetCore.Identity;

namespace DB
{
    public class APP_USER : IdentityUser
    {
        public string FullName { get; set; }
        public List<Address> Addresses { get; set; } = new List<Address>();
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
