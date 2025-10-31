using Microsoft.AspNetCore.Mvc;
using SellComputer.Data;
using SellComputer.Models.DTOs.Users;
using SellComputer.Models.Entities;

namespace SellComputer.Controllers
{
    public class UserController : BaseApiController
    {
        public UserController(ShopBanMayTinhContext dbContext) : base(dbContext)
        {
        }

        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = dbContext.Users.ToList();
            return Ok(users);
        }

        [HttpGet("{id:guid}")]
        public IActionResult GetUserByID(Guid id)
        {
            var user = dbContext.Users.Find(id);
            if ( user is null)
            {
                return NotFound();
            }
            return Ok(user);
        }


        [HttpPost] 
        public IActionResult AddUser(AddUserDto addUserDto)
        {
            if (addUserDto.RoleId.HasValue)
            {
                var roleExists = dbContext.Roles.Any(c => c.Id == addUserDto.RoleId.Value);
                if (!roleExists)
                {
                    return BadRequest(new
                    {
                        Error = " Mã chức vụ không tồn tại",
                        Message = "Chọn lại cho hợp lệ"
                    });
                }
            }
            var userEntity = new User()
            {
                Username = addUserDto.Username,
                Password = addUserDto.Password,
                FirstName = addUserDto.FirstName,
                LastName = addUserDto.LastName,
                Email = addUserDto.Email,
                Phone = addUserDto.Phone,
                Address = addUserDto.Address,
            };
            dbContext.Users.Add(userEntity);
            dbContext.SaveChanges();
            return Ok(userEntity);
        }
    }
}
