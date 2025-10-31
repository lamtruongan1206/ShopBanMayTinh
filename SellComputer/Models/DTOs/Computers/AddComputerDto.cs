using System.ComponentModel.DataAnnotations;

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

        public Guid? CategoriesId { get; set; }

    }
}
