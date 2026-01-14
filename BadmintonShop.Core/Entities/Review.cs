using System.ComponentModel.DataAnnotations;

namespace BadmintonShop.Core.Entities
{
    public class Review : BaseEntity
    {
        [Range(1, 5)]
        public int Rating { get; set; } // 1 đến 5 sao

        [Required]
        [MaxLength(500)]
        public string Comment { get; set; } // Nội dung bình luận

        public bool IsApproved { get; set; } = false; // Admin duyệt mới được hiện

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}