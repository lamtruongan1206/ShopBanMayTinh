using Microsoft.AspNetCore.Mvc;
using WebBanMayTinh.Models;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models.Entities;

namespace WebBanMayTinh.Controllers
{
    public class RegisterController : Controller
    {
        ShopBanMayTinhContext conn = new ShopBanMayTinhContext();
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterAction(string fullName, string phoneNumber, string address, string email, string password)
        {
            var existingUser = conn.Users.FirstOrDefault(u => u.Email == email);
            if (existingUser != null)
            {
                ViewBag.RegisterError = "Email đã được sử dụng.";
                return View("Register");
            }
            var userRole = conn.Roles.FirstOrDefault(r => r.Name == "User");
            if (userRole == null)
            {
                ViewBag.RegisterError = "Vai trò người dùng không tồn tại.";
                return View("Register");
            }
            User newUser = new User ()
            {
                Id = Guid.NewGuid(),
                Username = fullName,
                Email = email,
                Password = password,
                Address = address,
                Phone = phoneNumber,
                RoleId = conn.Roles.FirstOrDefault(r => r.Name == "User")!.Id,
            };
            conn.Users.Add(newUser);
            conn.SaveChanges();
            return RedirectToAction("Login", "Login");
        }


    }
}
