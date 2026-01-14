using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadmintonShop.Core.Entities
{
    public class Category : BaseEntity
    {
        [Required]
        public string Name { get; set; }
        public string Slug { get; set; }
        public bool IsShowOnHome { get; set; } = false;

        // Logic đệ quy danh mục cha-con
        public int? ParentId { get; set; }
        public Category Parent { get; set; }
        public ICollection<Category> Children { get; set; } = new List<Category>();
        public ICollection<Product> Products { get; set; }
    }
}