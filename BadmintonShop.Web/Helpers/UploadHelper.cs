using Microsoft.AspNetCore.Http;

namespace BadmintonShop.Web.Helpers
{
    public static class UploadHelper
    {
        public static async Task<string> UploadAsync(IFormFile file, string folder)
        {
            var path = Path.Combine("wwwroot/uploads", folder);
            Directory.CreateDirectory(path);

            var name = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(path, name);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{folder}/{name}";
        }
    }
}
