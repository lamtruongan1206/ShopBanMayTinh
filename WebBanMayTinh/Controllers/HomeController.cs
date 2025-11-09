using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;
using WebBanMayTinh.Models.Entities;

namespace WebBanMayTinh.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    ShopBanMayTinhContext conn = new ShopBanMayTinhContext();
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index(string searchName, string searchManufacturer, decimal? priceFrom, decimal? priceTo, int page = 1, int pageSize = 6)
    {
        // L?y d? li?u c? b?n
        var query = conn.Computers
            .Include(c => c.Categories)
            .Include(c => c.Images)
            .AsQueryable();

        // L?c theo tên
        if (!string.IsNullOrEmpty(searchName))
        {
            query = query.Where(c => c.Name.Contains(searchName));
        }

        // L?c theo hãng s?n xu?t
        if (!string.IsNullOrEmpty(searchManufacturer))
        {
            query = query.Where(c => c.Manufacturer.Contains(searchManufacturer));
        }

        // L?c theo kho?ng giá
        if (priceFrom.HasValue)
            query = query.Where(c => c.Price >= priceFrom.Value);
        if (priceTo.HasValue)
            query = query.Where(c => c.Price <= priceTo.Value);

        // T?ng s? b?n ghi sau l?c
        int totalItems = query.Count();

        // Phân trang
        var computers = query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Truy?n d? li?u phân trang qua ViewBag
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        // Truy?n l?i các giá tr? tìm ki?m ?? gi? trên form
        ViewBag.SearchName = searchName;
        ViewBag.SearchManufacturer = searchManufacturer;
        ViewBag.PriceFrom = priceFrom;
        ViewBag.PriceTo = priceTo;

        return View(computers);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
