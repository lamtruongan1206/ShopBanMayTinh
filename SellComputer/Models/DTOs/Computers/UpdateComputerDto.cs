using System.ComponentModel.DataAnnotations;

namespace SellComputer.Models.DTOs.Computers
{
    public class UpdateComputerDto
    {
       
        public string? Name { get; set; }

        public string? Manufacturer { get; set; }

        
        public decimal? Price { get; set; }

       
        public int? Quantity { get; set; }

        public Guid? CategoriesId { get; set; }
    }
}
