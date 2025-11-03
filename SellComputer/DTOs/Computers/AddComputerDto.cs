using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SellComputer.Models.DTOs.Computers
{
    public class AddComputerDto
    {
        [Required(ErrorMessage = "Tên máy tính là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Hãng sản xuất là bắt buộc")]
        public string Manufacturer { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
        public int Quantity { get; set; }
        public string? Description { get; set; }

        [Required(ErrorMessage = "Mã danh mục là bắt buộc")]
        public Guid? CategoriesId { get; set; }
        public IFormFile Images { get; set; }
        public List<IFormFile>? AdditionalImages { get; set; }
    }
}
