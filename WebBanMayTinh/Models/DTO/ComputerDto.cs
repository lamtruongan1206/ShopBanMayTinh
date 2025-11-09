using Microsoft.Identity.Client;

namespace WebBanMayTinh.Models.DTO
{
    public class ComputerDto
    {
        
             public string? Name { get; set; }

            public string? Manufacturer { get; set; }

            public decimal? Price { get; set; }

            public int? Quantity { get; set; }

            public DateOnly? UpdateAt { get; set; }

            public DateOnly? CreateAt { get; set; }
            
            public Guid? CategoriesId { get; set; }

            public string? Description { get; set; }

            public IFormFile? MainImage { get; set; }               // Ảnh chính
            public List<IFormFile>? AdditionalImages { get; set; }  // Ảnh phụ
    }
}
