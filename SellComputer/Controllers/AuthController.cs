using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SellComputer.Controllers;
using SellComputer.Data;
using SellComputer.Models.DTOs.Users;
using SellComputer.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
        var user = dbContext.Users.FirstOrDefault(u => u.Username == loginDto.Username && u.Password == loginDto.Password);
        if (user != null)
        {
            var role = dbContext.Roles.FirstOrDefault(r => r.Id == user.RoleId)?.Name;
            var token = GenerateJwtToken(user.Username ?? "", "User");
            return Ok(new { token, user, role });
        }

        return Unauthorized("Sai tài khoản hoặc mật khẩu");
    }

    [HttpPost("register")]
    public IActionResult SignUp([FromBody] AddUserDto userDto)
    {
        var role = dbContext.Roles.FirstOrDefault(r => r.Name == "USER");
        var RoleId = role?.Id;
        try
        {
            var user = new User()
            {
                Id = Guid.NewGuid(),
                Username = userDto.Username,
                Password = userDto.Password,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Phone = userDto.Phone,
                Address = userDto.Address,
                RoleId = RoleId
            };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();
            return Ok(user);
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
