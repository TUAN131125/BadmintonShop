using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    // -----------------------------------------------------------
    // QUAN TRỌNG:
    // AuthenticationSchemes = "AdminAuth": Bắt buộc controller này dùng Cookie của Admin
    // Roles = "Admin": Chỉ cho phép User có Role là "Admin"
    // -----------------------------------------------------------
    [Authorize(AuthenticationSchemes = "AdminAuth", Roles = "Admin")]
    public class BaseAdminController : Controller
    {
        // Tất cả Controller con (News, Product, User...) kế thừa từ đây
        // sẽ tự động được bảo vệ bởi "Cổng AdminAuth".
    }
}