using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DB
{
    public class Address
    {
        public int AddressId { get; set; }
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        public virtual APP_USER User { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string Zip { get; set; }
        public bool IsDefault { get; set; }
    }
}
