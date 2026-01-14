using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadmintonShop.Core.DTOs
{
    public class CheckoutItemDTO
    {
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }
}
