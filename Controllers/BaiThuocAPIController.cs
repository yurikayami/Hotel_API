using Hotel_API.Data;
using Hotel_API.Models;
using Hotel_API.Models.ViewModels;
using Hotel_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hotel_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BaiThuocAPIController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MediaUrlService _mediaUrlService;

        public BaiThuocAPIController(AppDbContext context, UserManager<ApplicationUser> userManager, MediaUrlService mediaUrlService)
        {
            _context = context;
            _userManager = userManager;
            _mediaUrlService = mediaUrlService;
        }

        /// <summary>
        /// Lấy danh sách bài thuốc
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetBaiThuocs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                var query = _context.BaiThuocs.Where(b => b.TrangThai == 1).OrderByDescending(b => b.NgayTao);

                var totalCount = await query.CountAsync();

                var baiThuocs = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(b => b.ApplicationUser)
                    .Select(b => new
                    {
                        b.Id,
                        b.Ten,
                        b.MoTa,
                        b.HuongDanSuDung,
                        b.NgayTao,
                        b.Image,
                        b.SoLuotThich,
                        b.SoLuotXem,
                        AuthorId = b.NguoiDungId,
                        AuthorName = b.ApplicationUser != null ? b.ApplicationUser.UserName : "Unknown",
                        AuthorAvatar = b.ApplicationUser != null ? b.ApplicationUser.ProfilePicture : null
                    })
                    .ToListAsync();

                // Convert image paths to full URLs
                var result = baiThuocs.Select(b => new
                {
                    b.Id,
                    b.Ten,
                    b.MoTa,
                    b.HuongDanSuDung,
                    b.NgayTao,
                    Image = _mediaUrlService.GetFullMediaUrl(b.Image?.ToString() ?? ""),
                    b.SoLuotThich,
                    b.SoLuotXem,
                    b.AuthorId,
                    b.AuthorName,
                    AuthorAvatar = _mediaUrlService.GetFullMediaUrl(b.AuthorAvatar?.ToString() ?? "")
                }).ToList();

                return Ok(new ApiResponse<List<object>>
                {
                    Success = true,
                    Message = "Lấy danh sách bài thuốc thành công",
                    Data = result.Cast<object>().ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<object>>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết bài thuốc
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> GetBaiThuoc(Guid id)
        {
            try
            {
                var baiThuoc = await _context.BaiThuocs
                    .Include(b => b.ApplicationUser)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (baiThuoc == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy bài thuốc"
                    });
                }

                // Increment view count
                baiThuoc.SoLuotXem = (baiThuoc.SoLuotXem ?? 0) + 1;
                await _context.SaveChangesAsync();

                var result = new
                {
                    baiThuoc.Id,
                    baiThuoc.Ten,
                    baiThuoc.MoTa,
                    baiThuoc.HuongDanSuDung,
                    baiThuoc.NgayTao,
                    Image = _mediaUrlService.GetFullMediaUrl(baiThuoc.Image),
                    baiThuoc.SoLuotThich,
                    baiThuoc.SoLuotXem,
                    AuthorId = baiThuoc.NguoiDungId,
                    AuthorName = baiThuoc.ApplicationUser?.UserName,
                    AuthorAvatar = _mediaUrlService.GetFullMediaUrl(baiThuoc.ApplicationUser?.ProfilePicture)
                };

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Lấy chi tiết bài thuốc thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Tạo bài thuốc mới
        /// </summary>
        [HttpPost("create")]
        [Authorize]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> CreateBaiThuoc()
        {
            try
            {
                var ten = Request.Form["ten"];
                var moTa = Request.Form["moTa"];
                var huongDanSuDung = Request.Form["huongDanSuDung"];
                var image = Request.Form.Files.GetFile("image");

                if (string.IsNullOrEmpty(ten))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Tên không được để trống"
                    });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Bạn cần đăng nhập để tạo bài thuốc"
                    });
                }

                string? imagePath = null;

                // Handle image upload
                if (image != null && image.Length > 0)
                {
                    const long maxFileSize = 5 * 1024 * 1024;
                    if (image.Length > maxFileSize)
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = "Kích thước file tối đa là 5MB"
                        });
                    }

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "baithuoc");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    imagePath = $"/uploads/baithuoc/{uniqueFileName}";
                }

                var baiThuoc = new BaiThuoc
                {
                    Id = Guid.NewGuid(),
                    Ten = ten.ToString() ?? string.Empty,
                    MoTa = moTa.ToString(),
                    HuongDanSuDung = huongDanSuDung.ToString(),
                    NguoiDungId = userId,
                    Image = imagePath,
                    NgayTao = DateTime.UtcNow,
                    SoLuotThich = 0,
                    SoLuotXem = 0,
                    TrangThai = 1
                };

                _context.BaiThuocs.Add(baiThuoc);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Tạo bài thuốc thành công",
                    Data = new
                    {
                        baiThuoc.Id,
                        baiThuoc.Ten,
                        baiThuoc.MoTa,
                        baiThuoc.HuongDanSuDung,
                        baiThuoc.NgayTao,
                        Image = _mediaUrlService.GetFullMediaUrl(baiThuoc.Image),
                        baiThuoc.SoLuotThich,
                        baiThuoc.SoLuotXem
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi tạo bài thuốc",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Lấy danh sách bài thuốc của chính mình (Authenticated - Infinity Scroll)
        /// </summary>
        /// <param name="offset">Số bài thuốc đã skip (mặc định 0)</param>
        /// <param name="limit">Số lượng bài thuốc trả về (mặc định 10, max 50)</param>
        [HttpGet("user/myMedicine")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> GetMyMedicines(
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 10)
        {
            try
            {
                if (offset < 0) offset = 0;
                if (limit < 1) limit = 10;
                if (limit > 50) limit = 50;

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Bạn cần đăng nhập để xem bài thuốc của mình"
                    });
                }

                var myMedicines = await _context.BaiThuocs
                    .Include(b => b.ApplicationUser)
                    .Where(b => b.NguoiDungId == userId)
                    .OrderByDescending(b => b.NgayTao)
                    .Skip(offset)
                    .Take(limit + 1)
                    .ToListAsync();

                var hasMore = myMedicines.Count > limit;
                if (hasMore)
                {
                    myMedicines = myMedicines.Take(limit).ToList();
                }

                var medicineDtos = myMedicines.Select(b => new
                {
                    b.Id,
                    b.Ten,
                    b.MoTa,
                    b.HuongDanSuDung,
                    b.NgayTao,
                    Image = _mediaUrlService.GetFullMediaUrl(b.Image),
                    b.SoLuotThich,
                    b.SoLuotXem,
                    AuthorId = b.NguoiDungId,
                    AuthorName = b.ApplicationUser?.UserName ?? "Unknown",
                    AuthorAvatar = _mediaUrlService.GetFullMediaUrl(b.ApplicationUser?.ProfilePicture)
                }).ToList();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Danh sách bài thuốc của bạn",
                    Data = new
                    {
                        medicines = medicineDtos,
                        hasMore = hasMore
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách bài thuốc",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Lấy danh sách bài thuốc của một người dùng (Public - Infinity Scroll)
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <param name="offset">Số bài thuốc đã skip (mặc định 0)</param>
        /// <param name="limit">Số lượng bài thuốc trả về (mặc định 10, max 50)</param>
        [HttpGet("public/{userId}/medicine")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<object>>> GetUserMedicines(
            string userId,
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "ID người dùng không được để trống"
                    });
                }

                if (offset < 0) offset = 0;
                if (limit < 1) limit = 10;
                if (limit > 50) limit = 50;

                var userMedicines = await _context.BaiThuocs
                    .Include(b => b.ApplicationUser)
                    .Where(b => b.NguoiDungId == userId && b.TrangThai == 1)
                    .OrderByDescending(b => b.NgayTao)
                    .Skip(offset)
                    .Take(limit + 1)
                    .ToListAsync();

                var hasMore = userMedicines.Count > limit;
                if (hasMore)
                {
                    userMedicines = userMedicines.Take(limit).ToList();
                }

                var medicineDtos = userMedicines.Select(b => new
                {
                    b.Id,
                    b.Ten,
                    b.MoTa,
                    b.HuongDanSuDung,
                    b.NgayTao,
                    Image = _mediaUrlService.GetFullMediaUrl(b.Image),
                    b.SoLuotThich,
                    b.SoLuotXem,
                    AuthorId = b.NguoiDungId,
                    AuthorName = b.ApplicationUser?.UserName ?? "Unknown",
                    AuthorAvatar = _mediaUrlService.GetFullMediaUrl(b.ApplicationUser?.ProfilePicture)
                }).ToList();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Danh sách bài thuốc của người dùng",
                    Data = new
                    {
                        medicines = medicineDtos,
                        hasMore = hasMore
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách bài thuốc",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}
