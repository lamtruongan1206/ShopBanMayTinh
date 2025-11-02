using Microsoft.EntityFrameworkCore;
using SellComputer.Data;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

// Đặt biến môi trường 
builder.Configuration.AddEnvironmentVariables();

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

// jwt config
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // chỉ false khi dev
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()   // Cho phép tất cả domain
            .AllowAnyMethod()   // Cho phép mọi HTTP method: GET, POST, PUT, DELETE...
            .AllowAnyHeader();  // Cho phép mọi header
    });
});


builder.Services.AddControllers();

// Add DbContext dùng connection string từ .env
builder.Services.AddDbContext<ShopBanMayTinhContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();
app.Run();
