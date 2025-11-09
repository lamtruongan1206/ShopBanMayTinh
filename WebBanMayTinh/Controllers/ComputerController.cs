using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;
using WebBanMayTinh.Models.DTO;
using WebBanMayTinh.Models.Entities;

namespace WebBanMayTinh.Controllers
{
    public class ComputerController : Controller
    {
        ShopBanMayTinhContext _context = new ShopBanMayTinhContext();


        // ================== INDEX ==================
        public IActionResult Index(string searchName, string searchManufacturer, decimal? priceFrom, decimal? priceTo, int page = 1, int pageSize = 6)
        {
            // Lấy dữ liệu cơ bản
            var query = _context.Computers
                .Include(c => c.Categories)
                .Include(c => c.Images)
                .AsQueryable();

            // Lọc theo tên
            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(c => c.Name.Contains(searchName));
            }

            // Lọc theo hãng sản xuất
            if (!string.IsNullOrEmpty(searchManufacturer))
            {
                query = query.Where(c => c.Manufacturer.Contains(searchManufacturer));
            }

            // Lọc theo khoảng giá
            if (priceFrom.HasValue)
                query = query.Where(c => c.Price >= priceFrom.Value);
            if (priceTo.HasValue)
                query = query.Where(c => c.Price <= priceTo.Value);

            // Tổng số bản ghi sau lọc
            int totalItems = query.Count();

            // Phân trang
            var computers = query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Truyền dữ liệu phân trang qua ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Truyền lại các giá trị tìm kiếm để giữ trên form
            ViewBag.SearchName = searchName;
            ViewBag.SearchManufacturer = searchManufacturer;
            ViewBag.PriceFrom = priceFrom;
            ViewBag.PriceTo = priceTo;

            return View(computers);
        }

        // ================== ADD ==================
        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.CategoriesId = new SelectList(_context.Categories, "Id", "Name");
            return View(new ComputerDto());
        }

        [HttpPost]
        public IActionResult AddAction(ComputerDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoriesId = new SelectList(_context.Categories, "Id", "Name", dto.CategoriesId);
                return View(dto);
            }

            if (dto.CategoriesId == null || !_context.Categories.Any(c => c.Id == dto.CategoriesId))
            {
                ModelState.AddModelError("CategoriesId", "Danh mục không tồn tại");
                ViewBag.CategoriesId = new SelectList(_context.Categories, "Id", "Name", dto.CategoriesId);
                return View(dto);
            }

            // Tạo entity Computer
            var computer = new Computer
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Manufacturer = dto.Manufacturer,
                Price = dto.Price,
                Quantity = dto.Quantity,
                Description = dto.Description,
                CategoriesId = dto.CategoriesId,
                CreateAt = DateOnly.FromDateTime(DateTime.Now),
                UpdateAt = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.Computers.Add(computer);

            // Upload ảnh
            string wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string folder = Path.Combine(wwwRoot, "assets", "img", "Computer");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            // Ảnh chính
            if (dto.MainImage != null)
            {
                string mainFileName = Guid.NewGuid() + Path.GetExtension(dto.MainImage.FileName);
                string mainPath = Path.Combine(folder, mainFileName);
                using (var fs = new FileStream(mainPath, FileMode.Create))
                {
                    dto.MainImage.CopyTo(fs);
                }
                _context.Images.Add(new Image
                {
                    Id = Guid.NewGuid(),
                    ProductId = computer.Id,
                    Url = $"/assets/img/Computer/{mainFileName}",
                    IsMain = true
                });
            }

            // Ảnh phụ
            if (dto.AdditionalImages != null && dto.AdditionalImages.Any())
            {
                foreach (var file in dto.AdditionalImages)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(folder, fileName);
                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fs);
                    }
                    _context.Images.Add(new Image
                    {
                        Id = Guid.NewGuid(),
                        ProductId = computer.Id,
                        Url = $"/assets/img/Computer/{fileName}",
                        IsMain = false
                    });
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // ================== UPDATE ==================
        [HttpGet]
        public IActionResult Update(Guid id)
        {
            var computer = _context.Computers
                .Include(c => c.Images)
                .FirstOrDefault(c => c.Id == id);

            if (computer == null) return NotFound();

            ViewBag.CategoriesId = new SelectList(_context.Categories, "Id", "Name", computer.CategoriesId);
            ViewBag.ComputerId = computer.Id;
            ViewBag.MainImage = computer.Images?.FirstOrDefault(i => i.IsMain)?.Url ?? "/images/default.png";
            var dto = new ComputerDto
            {
                Name = computer.Name,
                Manufacturer = computer.Manufacturer,
                Price = computer.Price,
                Quantity = computer.Quantity,
                Description = computer.Description,
                CategoriesId = computer.CategoriesId
            };
            return View(dto);
        }

        [HttpPost]
        public IActionResult UpdateAction(Guid id, ComputerDto dto)
        {
            var computer = _context.Computers
                .Include(c => c.Images)
                .FirstOrDefault(c => c.Id == id);

            if (computer == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.CategoriesId = new SelectList(_context.Categories, "Id", "Name", dto.CategoriesId);
                ViewBag.MainImage = computer.Images?.FirstOrDefault(i => i.IsMain)?.Url ?? "/images/default.png";
                return View("Update" , dto);
            }

            computer.Name = dto.Name;
            computer.Manufacturer = dto.Manufacturer;
            computer.Price = dto.Price;
            computer.Quantity = dto.Quantity;
            computer.Description = dto.Description;
            computer.CategoriesId = dto.CategoriesId;
            computer.UpdateAt = DateOnly.FromDateTime(DateTime.Now);

            string wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string folder = Path.Combine(wwwRoot, "assets", "img", "Computer");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            // Ảnh chính mới
            if (dto.MainImage != null)
            {
                var oldMain = computer.Images.FirstOrDefault(i => i.IsMain);
                if (oldMain != null)
                {
                    string oldPath = Path.Combine(wwwRoot, oldMain.Url.TrimStart('/').Replace("/", "\\"));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    _context.Images.Remove(oldMain);
                }

                string fileName = Guid.NewGuid() + Path.GetExtension(dto.MainImage.FileName);
                string filePath = Path.Combine(folder, fileName);
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    dto.MainImage.CopyTo(fs);
                }
                _context.Images.Add(new Image
                {
                    Id = Guid.NewGuid(),
                    ProductId = computer.Id,
                    Url = $"/assets/img/Computer/{fileName}",
                    IsMain = true
                });
            }

            // Ảnh phụ mới
            if (dto.AdditionalImages != null && dto.AdditionalImages.Any())
            {
                foreach (var file in dto.AdditionalImages)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(folder, fileName);
                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fs);
                    }
                    _context.Images.Add(new Image
                    {
                        Id = Guid.NewGuid(),
                        ProductId = computer.Id,
                        Url = $"/assets/img/Computer/{fileName}",
                        IsMain = false
                    });
                }
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // ================== DELETE ==================
        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var computer = _context.Computers.FirstOrDefault(c => c.Id == id);
            if (computer == null) return NotFound();
            return View(computer);
        }

        [HttpPost]
        public IActionResult DeleteAction(Guid id)
        {
            var computer = _context.Computers
                .Include(c => c.Images)
                .FirstOrDefault(c => c.Id == id);

            if (computer == null) return NotFound();

            string wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            foreach (var img in computer.Images)
            {
                string fullPath = Path.Combine(wwwRoot, img.Url.TrimStart('/').Replace("/", "\\"));
                if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
            }

            _context.Images.RemoveRange(computer.Images);
            _context.Computers.Remove(computer);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


        // ==================== DETAIL ===============
        [HttpGet]
        public IActionResult Detail(Guid id)
        {
            // Lấy máy tính theo id, bao gồm các ảnh
            var computer = _context.Computers
                .Include(c => c.Images)
                .Include(c => c.Categories) // Nếu muốn hiển thị tên danh mục
                .FirstOrDefault(c => c.Id == id);

            if (computer == null) return NotFound();

            // Truyền model vào view
            return View(computer);
        }


    }
}
