using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Controllers
{
    public class LoginController : Controller
    {
        ShopBanMayTinhContext conn = new ShopBanMayTinhContext();

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LoginAction(string email, string password)
        {
            var user = conn.Users.Include(u => u.Role)
                                 .FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                if (user.Role != null && user.Role.Name == "Admin")
                {
                    return RedirectToAction("Index", "Admin"); // chuyển sang trang Admin
                }

                // Chuyển sang trang User nếu không phải Admin
                return RedirectToAction("Index", "User");
            }

            // Nếu đăng nhập sai
            ViewBag.LoginError = "Email hoặc mật khẩu không đúng.";
            return View("Login");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Xử lý đăng xuất (nếu có session hoặc cookie, xóa chúng ở đây)
            return RedirectToAction("Login", "Login");
        }

    }
}
