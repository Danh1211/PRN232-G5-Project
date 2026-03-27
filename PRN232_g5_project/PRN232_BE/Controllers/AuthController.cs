using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232_BE.Models;
using PRN232_BE.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PRN232_BE.DTOs.Auth;

namespace PRN232_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly CloneEbayDb1Context _context;
        private readonly IConfiguration _configuration;

        // Tiêm cả DbContext và IConfiguration (để đọc JWT Secret Key từ appsettings.json)
        public AuthController(CloneEbayDb1Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Kiểm tra mật khẩu: ít nhất 8 ký tự, không chứa khoảng trắng
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8 || request.Password.Contains(" "))
            {
                return BadRequest(new { Message = "Password must be at least 8 characters long and contain no spaces." });
            }

            // Kiểm tra tính duy nhất của Username và Email
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest(new { Message = "Username already exists." });
            }
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { Message = "Email already exists." });
            }

            // Băm (Hash) mật khẩu trước khi lưu
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Tạo User mới với các giá trị mặc định theo yêu cầu
            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                Password = hashedPassword,
                Role = "User", // Mặc định là User
                AvatarUrl = "https://th.bing.com/th/id/OIP.yZvMziAUA939DB0zWaZcjwHaLH?w=186&h=279&c=7&r=0&o=7&dpr=1.4&pid=1.7&rm=3",
                Balance = 0,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Registration successful!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Tìm user bằng Username HOẶC Email
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Username == request.UsernameOrEmail || u.Email == request.UsernameOrEmail);

            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid username/email or password." });
            }

            bool isPasswordValid = false;

            // Null/empty guard
            if (string.IsNullOrEmpty(user.Password))
            {
                return Unauthorized(new { Message = "Invalid username/email or password." });
            }

            // If stored password looks like a bcrypt hash (starts with "$2"), use BCrypt.Verify safely
            if (user.Password.StartsWith("$2"))
            {
                try
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
                }
                catch (BCrypt.Net.SaltParseException)
                {
                    // malformed hash stored in DB - treat as invalid credential
                    isPasswordValid = false;
                }
            }
            else
            {
                // Legacy/plaintext password in DB: compare directly, then re-hash and save
                if (user.Password == request.Password)
                {
                    isPasswordValid = true;
                    // Re-hash with bcrypt and persist to DB to migrate to secure storage
                    user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }
            }

            if (!isPasswordValid)
            {
                return Unauthorized(new { Message = "Invalid username/email or password." });
            }

            // Tạo JWT Token
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                Message = "Login successful",
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                Role = user.Role
            });
        }

        // Hàm hỗ trợ tạo JWT Token
        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1), // Token có hạn 1 ngày
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}