using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SellComputer.Data;
using SellComputer.Models.DTOs.Computers;
using SellComputer.Models.Entities;
namespace SellComputer.Controllers
{
    public class ComputerController : BaseApiController
    {
        public ComputerController(ShopBanMayTinhContext dbContext) : base(dbContext)
        {
        }


        [HttpGet]
        public IActionResult GetAllComputers(int page = 1, int pageSize = 5)
        {
            // Đảm bảo page và pageSize hợp lệ
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;
            if (pageSize > 50) pageSize = 50;

            // Lấy tổng số máy tính
            var totalComputers = dbContext.Computers.Count();

            // Tính tổng số trang
            var totalPages = (int)Math.Ceiling(totalComputers / (double)pageSize);

            // Lấy dữ liệu cho trang hiện tại
            var computers = dbContext.Computers
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Trả về kết quả với thông tin phân trang
            return Ok(new
            {
                TotalCount = totalComputers,
                PageSize = pageSize,
                CurrentPage = page,
                TotalPages = totalPages,
                Computers = computers
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
        public IActionResult UpdateComputers(Guid id, [FromBody] UpdateComputerDto updateComputerDto)
        {
            var computer = dbContext.Computers.Find(id);
            if (computer is null)
            {
                return NotFound();
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

            dbContext.SaveChanges();
            return Ok(computer);
        }



        [HttpPost]
        public IActionResult AddComputer(AddComputerDto addComputerDto)
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

            var computerEntity = new Computer()
            {
                Name = addComputerDto.Name,
                Manufacturer = addComputerDto.Manufacturer,
                Price = addComputerDto.Price,
                Quantity = addComputerDto.Quantity,
                CategoriesId = addComputerDto.CategoriesId
            };

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
                    .Where(c =>
                        (c.Name != null && c.Name.ToLower().Contains(keywordLower)) ||
                        (c.Manufacturer != null && c.Manufacturer.ToLower().Contains(keywordLower)) ||
                        (c.Categories != null && c.Categories.Name != null && c.Categories.Name.ToLower().Contains(keywordLower)));

                // Lấy tổng số kết quả
                var totalComputers = query.Count();

                // Tính tổng số trang
                var totalPages = (int)Math.Ceiling(totalComputers / (double)pageSize);

                // Lấy dữ liệu cho trang hiện tại với thông tin Category đầy đủ
                var computers = query
                    .OrderBy(c => c.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new
                    {
                        Id = c.id,
                        Name = c.Name,
                        Manufacturer = c.Manufacturer,
                        Price = c.Price,
                        Quantity = c.Quantity,
                        CategoriesId = c.CategoriesId,
                        CategoryName = c.Categories != null ? c.Categories.Name : "Không có danh mục",
                        CategoryDescription = c.Categories != null ? c.Categories.Decription : null,
                        CreateAt = c.CreateAt,
                        UpdateAt = c.UpdateAt
                    })
                    .ToList();

                // Trả về kết quả với thông tin phân trang
                return Ok(new
                {
                    Keyword = keyword,
                    TotalCount = totalComputers,
                    PageSize = pageSize,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    Computers = computers
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
    }
}
