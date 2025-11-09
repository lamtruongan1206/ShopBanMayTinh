using Microsoft.AspNetCore.Mvc;
using WebBanMayTinh.Models.Entities;
using WebBanMayTinh.Models;
using Microsoft.EntityFrameworkCore;

namespace WebBanMayTinh.Controllers
{
    public class AdminController : Controller
    {
        ShopBanMayTinhContext conn = new ShopBanMayTinhContext();
        public IActionResult Index()
        {
            Computer computer = new Computer();
            var lst = conn.Computers.Include(b => b.Categories).ToList();
            return View(lst);
        }
    }
}
