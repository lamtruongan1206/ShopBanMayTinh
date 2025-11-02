namespace SellComputer.Models.DTOs.Computers
{
    public class ComputerDto
    {
        public string? Name { get; set; }

        public string? Manufacturer { get; set; }

        public decimal? Price { get; set; }

        public int? Quantity { get; set; }

        public DateTime? UpdateAt { get; set; }

        public DateTime? CreateAt { get; set; }

        public Guid? CategoriesId { get; set; }
        public string? Description { get; set; }
    }
}
