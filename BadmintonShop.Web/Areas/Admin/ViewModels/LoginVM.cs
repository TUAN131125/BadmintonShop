using System.ComponentModel.DataAnnotations;
namespace BadmintonShop.Web.Areas.Admin.ViewModels
{
    public class LoginVM
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
