using Hotel_API.Models;
using Hotel_API.Models.ViewModels;
using Hotel_API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Hotel_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly MediaUrlService _mediaUrlService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            MediaUrlService mediaUrlService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _mediaUrlService = mediaUrlService;
        }

        /// <summary>
        /// Đăng ký tài khoản mới
        /// </summary>
        /// <param name="model">Thông tin đăng ký</param>
        /// <returns>Thông tin user và JWT token</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ",
                });
            }

            // Kiểm tra email đã tồn tại
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Email đã được sử dụng"
                });
            }

            // Tạo user mới
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                tuoi = model.Age,
                gioi_tinh = model.Gender,
                ProfilePicture = "/images/avatar/default-profile-picture.jpg",
                dang_online = true,
                trang_thai = 1,
                lan_hoat_dong_cuoi = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Đăng ký thất bại",
                });
            }

            // Gán role User mặc định
            await _userManager.AddToRoleAsync(user, "User");

            // Tạo JWT token
            var token = GenerateJwtToken(user);

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Đăng ký thành công",
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    Age = user.tuoi,
                    Gender = user.gioi_tinh,
                    ProfilePicture = _mediaUrlService.GetFullMediaUrl(user.ProfilePicture),
                    DisplayName = user.displayName
                }
            });
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="model">Thông tin đăng nhập</param>
        /// <returns>Thông tin user và JWT token</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ"
                });
            }

            // Tìm user theo email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Email hoặc mật khẩu không đúng"
                });
            }

            // Kiểm tra password
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Email hoặc mật khẩu không đúng"
                });
            }

            // Cập nhật trạng thái online
            user.dang_online = true;
            user.trang_thai = 1;
            user.lan_hoat_dong_cuoi = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Tạo JWT token
            var token = GenerateJwtToken(user);

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Đăng nhập thành công",
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    Age = user.tuoi,
                    Gender = user.gioi_tinh,
                    ProfilePicture = _mediaUrlService.GetFullMediaUrl(user.ProfilePicture),
                    DisplayName = user.displayName
                }
            });
        }

        /// <summary>
        /// Đăng xuất (cập nhật trạng thái offline)
        /// </summary>
        /// <returns>Kết quả đăng xuất</returns>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    user.dang_online = false;
                    user.trang_thai = 0;
                    user.lan_hoat_dong_cuoi = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);
                }
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Đăng xuất thành công"
            });
        }

        #region Private Methods

        private string GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(jwtSettings["ExpiryInDays"])),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        #endregion
    }
}
