using Microsoft.AspNetCore.Mvc;
using SellComputer.Controllers;
using SellComputer.Data;
using SellComputer.Models.Entities;

namespace SellComputer.Controllers
{
    public class CategoryController : BaseApiController
    {

        public CategoryController(ShopBanMayTinhContext dbContext) : base(dbContext)
        {
        }

        [HttpGet]
        public IActionResult GetAllCategories()
        {
            var categories = dbContext.Categories.ToList();
            return Ok(categories);
        }

        [HttpPost]
        public IActionResult AddCategory([FromBody] AddCategoryDto categoryDto)
        {
            var category = new Category()
            {
                Id = Guid.NewGuid(),
                Name = categoryDto.Name,
                Description = categoryDto.Description
            };

            dbContext.Categories.Add(category);
            dbContext.SaveChanges();
            return Ok(category);
        }

    }

}
