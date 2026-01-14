using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadmintonShop.Core.Entities
{
    public class OrderDetail : BaseEntity
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal => Quantity * UnitPrice;
    }
}
