using Microsoft.AspNetCore.Http;

namespace BadmintonShop.Web.Areas.Admin.ViewModels.Banner
{
    public class BannerFormVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? SubTitle { get; set; }
        public string TargetUrl { get; set; } = "#";
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; } = true;

        public string? ExistingImage { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
