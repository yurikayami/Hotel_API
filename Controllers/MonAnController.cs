using Hotel_API.Data;
using Hotel_API.Models;
using Hotel_API.Models.ViewModels;
using Hotel_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MonAnController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly MediaUrlService _mediaUrlService;

        public MonAnController(AppDbContext context, MediaUrlService mediaUrlService)
        {
            _context = context;
            _mediaUrlService = mediaUrlService;
        }

        /// <summary>
        /// Lấy tất cả danh sách món ăn (không phân trang)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetMonAns()
        {
            try
            {
                var monAns = await _context.MonAns
                    .OrderBy(m => m.Ten)
                    .Select(m => new
                    {
                        m.Id,
                        m.Ten,
                        m.MoTa,
                        m.Gia,
                        m.Image,
                        Loai = m.Loai,
                        CachCheBien = m.CachCheBien,
                        m.SoNguoi,
                        m.LuotXem
                    })
                    .ToListAsync();

                // Convert image paths to full URLs
                var result = monAns.Select(m => new
                {
                    m.Id,
                    m.Ten,
                    m.MoTa,
                    m.Gia,
                    Image = _mediaUrlService.GetFullMediaUrl(m.Image ?? ""),
                    m.Loai,
                    m.CachCheBien,
                    m.SoNguoi,
                    m.LuotXem
                }).ToList();

                return Ok(new ApiResponse<List<object>>
                {
                    Success = true,
                    Message = $"Lấy danh sách {result.Count} món ăn thành công",
                    Data = result.Cast<object>().ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<object>>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra",
                    Data = null,
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Lấy danh sách món ăn với phân trang
        /// </summary>
        [HttpGet("paging")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> GetMonAnsPaging([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                var query = _context.MonAns.OrderBy(m => m.Ten);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var monAns = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(m => new
                    {
                        m.Id,
                        m.Ten,
                        m.MoTa,
                        m.Gia,
                        m.Image,
                        Loai = m.Loai,
                        CachCheBien = m.CachCheBien,
                        m.SoNguoi,
                        m.LuotXem
                    })
                    .ToListAsync();

                // Convert image paths to full URLs
                var items = monAns.Select(m => new
                {
                    m.Id,
                    m.Ten,
                    m.MoTa,
                    m.Gia,
                    Image = _mediaUrlService.GetFullMediaUrl(m.Image ?? ""),
                    m.Loai,
                    m.CachCheBien,
                    m.SoNguoi,
                    m.LuotXem
                }).ToList();

                var paginationData = new
                {
                    Items = items,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Lấy danh sách món ăn thành công",
                    Data = paginationData
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra",
                    Data = null,
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết món ăn theo ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> GetMonAn(Guid id)
        {
            try
            {
                var monAn = await _context.MonAns
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (monAn == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy món ăn",
                        Data = null
                    });
                }

                var result = new
                {
                    monAn.Id,
                    monAn.Ten,
                    monAn.MoTa,
                    monAn.Gia,
                    Image = _mediaUrlService.GetFullMediaUrl(monAn.Image),
                    Loai = monAn.Loai,
                    CachCheBien = monAn.CachCheBien,
                    monAn.SoNguoi,
                    monAn.LuotXem
                };

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Lấy chi tiết món ăn thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra",
                    Data = null,
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Lấy giá của món ăn
        /// </summary>
        [HttpGet("price/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> GetPrice(Guid id)
        {
            try
            {
                var monAn = await _context.MonAns
                    .Where(m => m.Id == id)
                    .Select(m => new { m.Id, m.Ten, m.Gia })
                    .FirstOrDefaultAsync();

                if (monAn == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy món ăn",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Lấy giá món ăn thành công",
                    Data = new
                    {
                        monAn.Id,
                        monAn.Ten,
                        monAn.Gia
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra",
                    Data = null,
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Tìm kiếm món ăn theo tên
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<object>>>> SearchMonAns([FromQuery] string keyword = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                var query = _context.MonAns.AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(m => m.Ten.Contains(keyword) ||
                                           (m.MoTa != null && m.MoTa.Contains(keyword)) ||
                                           (m.Loai != null && m.Loai.Contains(keyword)));
                }
                query = query.OrderBy(m => m.Ten);

                var totalCount = await query.CountAsync();

                var monAns = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(m => new
                    {
                        m.Id,
                        m.Ten,
                        m.MoTa,
                        m.Gia,
                        m.Image,
                        m.Loai
                    })
                    .ToListAsync();

                var result = monAns.Select(m => new
                {
                    m.Id,
                    m.Ten,
                    m.MoTa,
                    m.Gia,
                    Image = _mediaUrlService.GetFullMediaUrl(m.Image ?? ""),
                    m.Loai
                }).ToList();

                return Ok(new ApiResponse<List<object>>
                {
                    Success = true,
                    Message = $"Tìm thấy {totalCount} món ăn",
                    Data = result.Cast<object>().ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<object>>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra",
                    Data = null,
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Lấy món ăn đề xuất (recommended)
        /// </summary>
        [HttpGet("recommended")]
        [ProducesResponseType(typeof(ApiResponse<List<object>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<object>>>> GetRecommended([FromQuery] int limit = 5)
        {
            try
            {
                if (limit < 1 || limit > 20) limit = 5;

                // Lấy ngẫu nhiên các món ăn
                var monAns = await _context.MonAns
                    .OrderBy(x => Guid.NewGuid())
                    .Take(limit)
                    .Select(m => new
                    {
                        m.Id,
                        m.Ten,
                        m.MoTa,
                        m.Gia,
                        m.Image,
                        m.Loai
                    })
                    .ToListAsync();

                var result = monAns.Select(m => new
                {
                    m.Id,
                    m.Ten,
                    m.MoTa,
                    m.Gia,
                    Image = _mediaUrlService.GetFullMediaUrl(m.Image ?? ""),
                    m.Loai
                }).ToList();

                return Ok(new ApiResponse<List<object>>
                {
                    Success = true,
                    Message = "Lấy món ăn đề xuất thành công",
                    Data = result.Cast<object>().ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<object>>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra",
                    Data = null,
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}
