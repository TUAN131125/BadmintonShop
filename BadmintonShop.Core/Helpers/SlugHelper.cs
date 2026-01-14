using System.Text;
using System.Text.RegularExpressions;

namespace BadmintonShop.Core.Helpers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = input.ToLower().Trim();

            // Remove Vietnamese accents
            input = Regex.Replace(input, @"[áàảãạăắằẳẵặâấầẩẫậ]", "a");
            input = Regex.Replace(input, @"[éèẻẽẹêếềểễệ]", "e");
            input = Regex.Replace(input, @"[íìỉĩị]", "i");
            input = Regex.Replace(input, @"[óòỏõọôốồổỗộơớờởỡợ]", "o");
            input = Regex.Replace(input, @"[úùủũụưứừửữự]", "u");
            input = Regex.Replace(input, @"[ýỳỷỹỵ]", "y");
            input = Regex.Replace(input, @"đ", "d");

            // Remove invalid chars
            input = Regex.Replace(input, @"[^a-z0-9\s-]", "");

            // Replace spaces with -
            input = Regex.Replace(input, @"\s+", "-");

            return input;
        }
    }
}
