using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SellComputer.Controllers;
using SellComputer.Data;
using SellComputer.Models.DTOs.Users;
using SellComputer.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using System.Runtime.ExceptionServices;

[Route("api/[controller]")]
[ApiController]
public class AuthController : BaseApiController
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config, ShopBanMayTinhContext dbContext) : base(dbContext)
    {
        _config = config;
    }

    [HttpPost("login")]
    public IActionResult Login(LoginDto loginDto)
    {
        //var user = dbContext.Users.FirstOrDefault(u => u.Email == loginDto.Email && u.Password == loginDto.Password);
        //if (user != null)
        //{
        //    var role = dbContext.Roles.FirstOrDefault(r => r.Id == user.RoleId)?.Name;
        //    var token = GenerateJwtToken(user.Username ?? "", "User");
        //    return Ok(new { token, user, role });
        //}

        //return Unauthorized("Sai tài khoản hoặc mật khẩu");

        var user = dbContext.Users.FirstOrDefault(u => u.Email == loginDto.Email);
        if (user == null)
        {
            return Unauthorized("Sai tài khoản hoặc mật khẩu");
        }

        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
        {
            return Unauthorized("Sai tài khoản hoặc mật khẩu");
        }
        var role = dbContext.Roles.FirstOrDefault(r => r.Id == user.RoleId)?.Name;
        var token = GenerateJwtToken(user.Username ?? "", "User");
        return Ok(new { token, user, role });
    }



    [HttpPost("register")]
    public IActionResult SignUp([FromBody] AddUserDto userDto)
    {
        if(dbContext.Users.Any(u => u.Email == userDto.Email))
        {
            return BadRequest("Email đã được sử dụng !");
        }
        var role = dbContext.Roles.FirstOrDefault(r => r.Name == "User");
        var RoleId = role?.Id;
        try
        {
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
        var user = new User()
            {
                Id = Guid.NewGuid(),
                Username = userDto.Username,
                Password = passwordHash,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Phone = userDto.Phone,
                Address = userDto.Address,
                RoleId = RoleId
            };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();
            return Ok(new
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                RoleId = user.RoleId
            });
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex.Message);
            return BadRequest("Đăng ký tài khoản không thành công !");
        }
    }

    [HttpGet("me")]
    public IActionResult GetInfoUser()
    {
        var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized("Thiếu hoặc không hợp lệ token trong header Authorization!");
        }
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var username = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        // var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        var user = dbContext.Users.FirstOrDefault(u => u.Username == username);
        var role = dbContext.Roles.FirstOrDefault(r => r.Id == user.RoleId)?.Name;
        if (user == null)
        {
            return Unauthorized("Không tìm thấy user");
        }

        return Ok(new { user, role });
    }

    private string GenerateJwtToken(string username, string role)
    {
        var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
