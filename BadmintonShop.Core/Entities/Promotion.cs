using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BadmintonShop.Core.Entities
{
    public class Promotion : BaseEntity
    {
        [Required]
        public string Name { get; set; }        // Tên chương trình (VD: Sale Hè 2024)
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal DiscountValue { get; set; } // Giá trị giảm
        public bool IsPercent { get; set; } = true; // True: Giảm %, False: Giảm tiền mặt

        public bool IsActive { get; set; } = true;

        // Quan hệ Many-to-Many với Product
        public ICollection<PromotionProduct> PromotionProducts { get; set; }
    }
}