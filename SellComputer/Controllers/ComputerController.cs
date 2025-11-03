using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SellComputer.Data;
using SellComputer.Models.DTOs.Computers;
using SellComputer.Models.Entities;
using Microsoft.AspNetCore.Http;
namespace SellComputer.Controllers
{
    public class ComputerController : BaseApiController
    {
        public ComputerController(ShopBanMayTinhContext dbContext) : base(dbContext)
        {
        }


        [HttpGet]
        public IActionResult GetAllComputers(int page = 1, int pageSize = 8)
        {
            // Đảm bảo page và pageSize hợp lệ
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 8;
            if (pageSize > 50) pageSize = 80;

            // Lấy tổng số máy tính
            var totalComputers = dbContext.Computers.Count();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling(totalComputers / (double)pageSize);

            // Lấy dữ liệu cho trang hiện tại
            var computers = dbContext.Computers
                .Include(b => b.Categories)
                .Include(b => b.Images)
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

           // Đưa main image lên đầu danh sách ảnh
            var result = computers.Select(c => new
            {
                c.Id,
                c.Name,
                c.Price,
                c.Description,
                c.Categories,
                c.Quantity,
                c.CategoriesId,
                c.Manufacturer,
                // Ảnh được sắp xếp: IsMain lên đầu
                Images = c.Images
            .OrderByDescending(i => i.IsMain)
            .ThenBy(i => i.Id) // hoặc thêm ThenBy(i => i.CreatedAt) nếu có
            .Select(i => new
            {
                i.Id,
                i.Url,
                i.IsMain
            })
            .ToList()
            });


            // Trả về kết quả với thông tin phân trang
            return Ok(new
            {
                TotalCount = totalComputers,
                PageSize = pageSize,
                CurrentPage = page,
                TotalPages = totalPages,
                Data = result
            });
        }


        [HttpGet("{id:guid}")]

        public IActionResult GetComputerById(Guid id)
        {
            var computer = dbContext.Computers.Find(id);
            if (computer is null)
            {
                return NotFound();
            }
            return Ok(computer);
        }


        [HttpPatch("{id:guid}")]
        public IActionResult UpdateComputers(Guid id, [FromForm] UpdateComputerDto updateComputerDto)
        {
            var computer = dbContext.Computers
                .Include(c => c.Images) // Quan trọng: include images để load ảnh hiện tại
                .FirstOrDefault(c => c.Id == id);

            if (computer is null)
            {
                return NotFound(new { Error = "Máy tính không tồn tại" });
            }

            // Validate CategoriesId tồn tại (nếu có giá trị)
            if (updateComputerDto.CategoriesId.HasValue)
            {
                var categoryExists = dbContext.Categories.Any(c => c.Id == updateComputerDto.CategoriesId.Value);
                if (!categoryExists)
                {
                    return BadRequest("Mã danh mục không tồn tại trong hệ thống");
                }
                computer.CategoriesId = updateComputerDto.CategoriesId.Value;
            }

            // Cập nhật các trường khác
            if (!string.IsNullOrEmpty(updateComputerDto.Name))
                computer.Name = updateComputerDto.Name;

            if (!string.IsNullOrEmpty(updateComputerDto.Manufacturer))
                computer.Manufacturer = updateComputerDto.Manufacturer;

            if (updateComputerDto.Price.HasValue)
                computer.Price = updateComputerDto.Price.Value;

            if (updateComputerDto.Quantity.HasValue)
                computer.Quantity = updateComputerDto.Quantity.Value;

            //  Xử lý xóa ảnh phụ
            if (updateComputerDto.DeletedImageIds != null && updateComputerDto.DeletedImageIds.Any())
            {
                var imagesToDelete = dbContext.Images
                    .Where(i => updateComputerDto.DeletedImageIds.Contains(i.Id) && i.ProductId == id)
                    .ToList();

                foreach (var img in imagesToDelete)
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.Url.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);

                    dbContext.Images.Remove(img);
                }
            }

            //  Thêm ảnh phụ mới
            if (updateComputerDto.AdditionalImages != null && updateComputerDto.AdditionalImages.Any())
            {
                foreach (var file in updateComputerDto.AdditionalImages)
                {
                    var ext = Path.GetExtension(file.FileName).ToLower();
                    var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    if (!allowed.Contains(ext))
                        return BadRequest("Chỉ cho phép ảnh JPG, PNG, GIF, WEBP");

                    var fileName = Guid.NewGuid() + ext;
                    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                    var fullPath = Path.Combine(folder, fileName);

                    using (var fs = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(fs);
                    }

                    dbContext.Images.Add(new Image
                    {
                        Id = Guid.NewGuid(),
                        ProductId = computer.Id,
                        Url = $"/images/{fileName}",
                        IsMain = false
                    });
                }
            }

            if (updateComputerDto.Images != null && updateComputerDto.Images.Length > 0)
            {
                // Validate ảnh
                if (updateComputerDto.Images.Length > 5 * 1024 * 1024) // 5MB
                {
                    return BadRequest(new { Error = "Ảnh không được vượt quá 5MB" });
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(updateComputerDto.Images.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { Error = "Chỉ chấp nhận file ảnh JPG, PNG, GIF" });
                }

                // Tìm ảnh cũ (ảnh chính) để xóa
                var oldMainImage = computer.Images.FirstOrDefault(img => img.IsMain);
                if (oldMainImage != null)
                {
                    // Xóa file vật lý cũ
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldMainImage.Url.TrimStart('/'));
                    // Xóa record cũ trong database
                    dbContext.Images.Remove(oldMainImage);
                }

                // Upload ảnh mới
                var imageName = Guid.NewGuid() + Path.GetExtension(updateComputerDto.Images.FileName);
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", imageName);
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                using (var fileStream = new FileStream(imagePath, FileMode.Create))
                {
                    updateComputerDto.Images.CopyTo(fileStream);
                }

                // Tạo image entity mới
                var imageEntity = new Image
                {
                    Id = Guid.NewGuid(),
                    ProductId = computer.Id,
                    Url = $"/images/{imageName}",
                    IsMain = true
                };

                dbContext.Images.Add(imageEntity);
            }

            dbContext.SaveChanges();
            return Ok(computer);
        }



        [HttpPost]
        public IActionResult AddComputer([FromForm] AddComputerDto addComputerDto)
        {
            // Validate CategoriesId tồn tại
            if (addComputerDto.CategoriesId.HasValue)
            {
                var categoryExists = dbContext.Categories.Any(c => c.Id == addComputerDto.CategoriesId.Value);
                if (!categoryExists)
                {
                    return BadRequest(new
                    {
                        Error = "Mã danh mục không tồn tại",
                        Message = "Vui lòng chọn mã danh mục hợp lệ từ danh sách có sẵn"
                    });
                }
            }
            var imageName = Guid.NewGuid() + Path.GetExtension(addComputerDto.Images.FileName);
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", imageName);
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);


            using (var fileStream = new FileStream(imagePath, FileMode.Create))
            {
                addComputerDto.Images.CopyTo(fileStream);
            }
            var computerEntity = new Computer()
            {
                Id = Guid.NewGuid(),
                Name = addComputerDto.Name,
                Manufacturer = addComputerDto.Manufacturer,
                Price = addComputerDto.Price,
                Quantity = addComputerDto.Quantity,
                CategoriesId = addComputerDto.CategoriesId,
                Description = addComputerDto.Description
            };
            var imgageEntity = new Image
            {
                Id = Guid.NewGuid(),
                ProductId = computerEntity.Id,
                Product = computerEntity,
                Url = $"/images/{imageName}",
                IsMain = true
            };
            dbContext.Images.Add(imgageEntity);
            dbContext.Computers.Add(computerEntity);
            dbContext.SaveChanges();
            return Ok(computerEntity);
        }


        [HttpDelete("{id:guid}")]
        public IActionResult DeleteComputer(Guid id)
        {
            var computer = dbContext.Computers.Find(id);
            if (computer is null)
            {
                return NotFound();
            }
            // Remove all image references from the computer
            var imgs = dbContext.Images.Where(img => img.ProductId == id).ToList();
            if (imgs.Count > 0)
            {
                foreach (var img in imgs)
                {
                    dbContext.Images.Remove(img);
                }
            }

            dbContext.Computers.Remove(computer);
            dbContext.SaveChanges();
            return Ok(computer);
        }


        [HttpGet("search")]
        public IActionResult SearchComputers(
            [FromQuery] string keyword,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return BadRequest("Từ khóa tìm kiếm không được để trống");
                }

                // Đảm bảo page và pageSize hợp lệ
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 5;
                if (pageSize > 50) pageSize = 50;

                // Chuyển keyword về chữ thường để tìm kiếm không phân biệt hoa thường
                var keywordLower = keyword.ToLower();

                // Tìm kiếm với JOIN bảng Categories và không phân biệt hoa thường
                var query = dbContext.Computers
                    .Include(c => c.Categories)
                    .Include(c => c.Images)
                    .Where(c =>
                        (c.Name != null && c.Name.ToLower().Contains(keywordLower)) ||
                        (c.Manufacturer != null && c.Manufacturer.ToLower().Contains(keywordLower)) ||
                        (c.Categories != null && c.Categories.Name != null && c.Categories.Name.ToLower().Contains(keywordLower)));

                // Lấy tổng số kết quả
                var totalComputers = query.Count();

                // Tính tổng số trang
                var totalPages = (int)Math.Ceiling(totalComputers / (double)pageSize);

                // Lấy dữ liệu cho trang hiện tại với thông tin Category đầy đủ
                // var computers = query
                //     .OrderBy(c => c.Name)
                //     .Skip((page - 1) * pageSize)
                //     .Take(pageSize)
                //     .Select(c => new
                //     {
                //         Id = c.Id,
                //         Name = c.Name,
                //         Manufacturer = c.Manufacturer,
                //         Price = c.Price,
                //         Quantity = c.Quantity,
                //         CategoriesId = c.CategoriesId,
                //         CategoryName = c.Categories != null ? c.Categories.Name : "Không có danh mục",
                //         CategoryDescription = c.Categories != null ? c.Categories.Description : null,
                //         CreateAt = c.CreateAt,
                //         UpdateAt = c.UpdateAt,
                //         Images = c.Images
                //     })
                //     .ToList();

                // Trả về kết quả với thông tin phân trang
                return Ok(new
                {
                    Keyword = keyword,
                    TotalCount = totalComputers,
                    PageSize = pageSize,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    Data = query
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "Lỗi khi tìm kiếm",
                    Message = ex.Message
                });
            }
        }

        [HttpPost("upload-image/{computerId}")]
        public IActionResult UploadImage(IFormFile image, Guid computerId)
        {
            var computer = dbContext.Computers.Find(computerId);
            System.Console.WriteLine("ComputerId: " + computerId);
            if (computer is null)
            {
                return NotFound("Máy tính không tồn tại trong hệ thống");
            }

            var imageName = Guid.NewGuid() + Path.GetExtension(image.FileName);
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", imageName);
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);


            using (var fileStream = new FileStream(imagePath, FileMode.Create))
            {
                image.CopyTo(fileStream);
            }
            var imageEntity = new Image()
            {
                Id = Guid.NewGuid(),
                ProductId = computerId,
                Product = computer,
                Url = $"/images/{imageName}",
            };
            dbContext.Images.Add(imageEntity);
            dbContext.SaveChanges();

            return Ok(imageEntity);
        }
    }
}
