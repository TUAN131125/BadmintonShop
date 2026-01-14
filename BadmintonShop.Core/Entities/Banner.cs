namespace BadmintonShop.Core.Entities
{
    public class Banner : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string SubTitle { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;   // <-- chỉ lưu path ảnh
        public string TargetUrl { get; set; } = string.Empty;

        public int OrderIndex { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
