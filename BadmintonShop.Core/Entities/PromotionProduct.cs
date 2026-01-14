using System.ComponentModel.DataAnnotations.Schema;

namespace BadmintonShop.Core.Entities
{
    // Bảng này không cần BaseEntity vì nó dùng khóa chính phức hợp (Composite Key)
    public class PromotionProduct
    {
        public int PromotionId { get; set; }
        public Promotion Promotion { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}