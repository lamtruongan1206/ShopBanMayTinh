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
            if (user is null)
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

            var existingEmail = dbContext.Users.FirstOrDefault(u => u.Email == addUserDto.Email);
            if (existingEmail != null)
            {
                return BadRequest(new
                {
                    Error = "Email đã được đăng ký",
                    Message = "Vui lòng sử dụng email khác"
                });
            }
            if (addUserDto.Password.Length < 6)
                return BadRequest(new { Error = "Password phải có ít nhất 6 ký tự" });

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(addUserDto.Password);

            var userEntity = new User()
            {
                Username = addUserDto.Username,
                Password = passwordHash,
                FirstName = addUserDto.FirstName,
                LastName = addUserDto.LastName,
                Email = addUserDto.Email,
                Phone = addUserDto.Phone,
                Address = addUserDto.Address,
                RoleId = addUserDto.RoleId,
            };
            dbContext.Users.Add(userEntity);
            dbContext.SaveChanges();
            return Ok(new
            {
                Id = userEntity.Id,
                Username = userEntity.Username,
                FirstName = userEntity.FirstName,
                LastName = userEntity.LastName,
                Email = userEntity.Email,
                Phone = userEntity.Phone,
                Address = userEntity.Address,
                RoleId = userEntity.RoleId,
            });
        }

    }
}
