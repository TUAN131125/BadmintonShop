using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadmintonShop.Core.DTOs
{
    public class CheckoutDTO
    {
        public int? UserId { get; set; }   // null = guest
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ShippingAddress { get; set; }
        public decimal ShippingFee { get; set; } = 0;
        public string PaymentMethod { get; set; } // COD | PAYPAL
        public List<CheckoutItemDTO> Items { get; set; }
        public string City { get; set; }
        public string Ward { get; set; }
    }
}
