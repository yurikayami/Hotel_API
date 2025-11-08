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
    public class PostController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MediaUrlService _mediaUrlService;

        public PostController(AppDbContext context, UserManager<ApplicationUser> userManager, MediaUrlService mediaUrlService)
        {
            _context = context;
            _userManager = userManager;
            _mediaUrlService = mediaUrlService;
        }

        /// <summary>
        /// Lấy danh sách bài viết có phân trang
        /// </summary>
        /// <param name="page">Trang hiện tại (mặc định 1)</param>
        /// <param name="pageSize">Số bài viết mỗi trang (mặc định 10)</param>
        /// <returns>Danh sách bài viết</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PostPagedResult>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PostPagedResult>>> GetPosts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var query = _context.BaiDang
                    .Include(p => p.ApplicationUser)
                    .Where(p => p.DaDuyet == true || p.DaDuyet == null)
                    .OrderByDescending(p => p.NgayDang);

                var totalCount = await query.CountAsync();

                var posts = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new PostDto
                    {
                        Id = p.Id,
                        NoiDung = p.NoiDung ?? string.Empty,
                        Loai = p.Loai,
                        DuongDanMedia = p.DuongDanMedia,
                        NgayDang = p.NgayDang,
                        LuotThich = p.LuotThich ?? 0,
                        SoBinhLuan = p.SoBinhLuan ?? 0,
                        SoChiaSe = p.so_chia_se,
                        Hashtags = p.hashtags,
                        IsLiked = userId != null && _context.BaiDang_LuotThich
                            .Any(l => l.baidang_id == p.Id && l.nguoidung_id == userId),
                        AuthorId = p.NguoiDungId ?? string.Empty,
                        AuthorName = p.ApplicationUser != null ? p.ApplicationUser.UserName! : "Unknown",
                        AuthorAvatar = p.ApplicationUser != null ? p.ApplicationUser.ProfilePicture : null
                    })
                    .ToListAsync();

                // Chuyển đổi đường dẫn media thành URL đầy đủ
                foreach (var post in posts)
                {
                    post.DuongDanMedia = _mediaUrlService.GetFullMediaUrl(post.DuongDanMedia);
                    post.AuthorAvatar = _mediaUrlService.GetFullMediaUrl(post.AuthorAvatar);
                }

                var result = new PostPagedResult
                {
                    Posts = posts,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };

                return Ok(new ApiResponse<PostPagedResult>
                {
                    Success = true,
                    Message = "Lấy danh sách bài viết thành công",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PostPagedResult>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách bài viết",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết một bài viết
        /// </summary>
        /// <param name="id">ID của bài viết</param>
        /// <returns>Chi tiết bài viết</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<PostDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PostDto>>> GetPost(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var post = await _context.BaiDang
                    .Include(p => p.ApplicationUser)
                    .Where(p => p.Id == id)
                    .Select(p => new PostDto
                    {
                        Id = p.Id,
                        NoiDung = p.NoiDung ?? string.Empty,
                        Loai = p.Loai,
                        DuongDanMedia = p.DuongDanMedia,
                        NgayDang = p.NgayDang,
                        LuotThich = p.LuotThich ?? 0,
                        SoBinhLuan = p.SoBinhLuan ?? 0,
                        SoChiaSe = p.so_chia_se,
                        Hashtags = p.hashtags,
                        IsLiked = userId != null && _context.BaiDang_LuotThich
                            .Any(l => l.baidang_id == p.Id && l.nguoidung_id == userId),
                        AuthorId = p.NguoiDungId ?? string.Empty,
                        AuthorName = p.ApplicationUser != null ? p.ApplicationUser.UserName! : "Unknown",
                        AuthorAvatar = p.ApplicationUser != null ? p.ApplicationUser.ProfilePicture : null
                    })
                    .FirstOrDefaultAsync();

                if (post == null)
                {
                    return NotFound(new ApiResponse<PostDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy bài viết"
                    });
                }

                // Chuyển đổi đường dẫn media thành URL đầy đủ
                post.DuongDanMedia = _mediaUrlService.GetFullMediaUrl(post.DuongDanMedia);
                post.AuthorAvatar = _mediaUrlService.GetFullMediaUrl(post.AuthorAvatar);

                return Ok(new ApiResponse<PostDto>
                {
                    Success = true,
                    Message = "Lấy chi tiết bài viết thành công",
                    Data = post
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PostDto>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy chi tiết bài viết",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Tạo bài viết mới
        /// </summary>
        /// <param name="model">Thông tin bài viết</param>
        /// <returns>Bài viết vừa tạo</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PostDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<PostDto>>> CreatePost([FromBody] CreatePostDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<PostDto>
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ"
                    });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse<PostDto>
                    {
                        Success = false,
                        Message = "Bạn cần đăng nhập để tạo bài viết"
                    });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Unauthorized(new ApiResponse<PostDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin người dùng"
                    });
                }

                var newPost = new BaiDang
                {
                    Id = Guid.NewGuid(),
                    NguoiDungId = userId,
                    NoiDung = model.NoiDung,
                    Loai = model.Loai,
                    DuongDanMedia = model.DuongDanMedia,
                    NgayDang = DateTime.UtcNow,
                    Id_MonAn = model.MonAnId,
                    hashtags = model.Hashtags,
                    LuotThich = 0,
                    SoBinhLuan = 0,
                    so_chia_se = 0,
                    DaDuyet = true,
                    NguoiDang = user.UserName
                };

                _context.BaiDang.Add(newPost);
                await _context.SaveChangesAsync();

                var postDto = new PostDto
                {
                    Id = newPost.Id,
                    NoiDung = newPost.NoiDung ?? string.Empty,
                    Loai = newPost.Loai,
                    DuongDanMedia = newPost.DuongDanMedia,
                    NgayDang = newPost.NgayDang,
                    LuotThich = newPost.LuotThich ?? 0,
                    SoBinhLuan = newPost.SoBinhLuan ?? 0,
                    SoChiaSe = newPost.so_chia_se,
                    Hashtags = newPost.hashtags,
                    IsLiked = false,
                    AuthorId = userId,
                    AuthorName = user.UserName!,
                    AuthorAvatar = user.ProfilePicture
                };

                // Chuyển đổi đường dẫn media thành URL đầy đủ
                postDto.DuongDanMedia = _mediaUrlService.GetFullMediaUrl(postDto.DuongDanMedia);
                postDto.AuthorAvatar = _mediaUrlService.GetFullMediaUrl(postDto.AuthorAvatar);

                return CreatedAtAction(nameof(GetPost), new { id = newPost.Id }, new ApiResponse<PostDto>
                {
                    Success = true,
                    Message = "Tạo bài viết thành công",
                    Data = postDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PostDto>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi tạo bài viết",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Like/Unlike bài viết
        /// </summary>
        /// <param name="id">ID của bài viết</param>
        /// <returns>Kết quả like</returns>
        [HttpPost("{id}/like")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> LikePost(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Bạn cần đăng nhập để thích bài viết"
                    });
                }

                var post = await _context.BaiDang.FindAsync(id);
                if (post == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy bài viết"
                    });
                }

                var existingLike = await _context.BaiDang_LuotThich
                    .FirstOrDefaultAsync(l => l.baidang_id == id && l.nguoidung_id == userId);

                if (existingLike != null)
                {
                    // Unlike
                    _context.BaiDang_LuotThich.Remove(existingLike);
                    post.LuotThich = (post.LuotThich ?? 1) - 1;
                    await _context.SaveChangesAsync();

                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Đã bỏ thích bài viết",
                        Data = new { isLiked = false, likeCount = post.LuotThich }
                    });
                }
                else
                {
                    // Like
                    var newLike = new BaiDang_LuotThich
                    {
                        id = Guid.NewGuid(),
                        baidang_id = id,
                        nguoidung_id = userId,
                        ngay_thich = DateTime.UtcNow
                    };

                    _context.BaiDang_LuotThich.Add(newLike);
                    post.LuotThich = (post.LuotThich ?? 0) + 1;
                    await _context.SaveChangesAsync();

                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Đã thích bài viết",
                        Data = new { isLiked = true, likeCount = post.LuotThich }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi xử lý like bài viết",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Lấy danh sách comment của bài viết
        /// </summary>
        /// <param name="id">ID của bài viết</param>
        /// <returns>Danh sách comment</returns>
        [HttpGet("{id}/comments")]
        [ProducesResponseType(typeof(ApiResponse<List<CommentDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<CommentDto>>>> GetComments(Guid id)
        {
            try
            {
                var post = await _context.BaiDang.FindAsync(id);
                if (post == null)
                {
                    return NotFound(new ApiResponse<List<CommentDto>>
                    {
                        Success = false,
                        Message = "Không tìm thấy bài viết"
                    });
                }

                var comments = await _context.BinhLuan
                    .Include(c => c.ApplicationUser)
                    .Where(c => c.BaiDangId == id && c.ParentCommentId == null)
                    .OrderByDescending(c => c.NgayTao)
                    .Select(c => new CommentDto
                    {
                        Id = c.Id,
                        NoiDung = c.NoiDung ?? string.Empty,
                        NgayTao = c.NgayTao,
                        ParentCommentId = c.ParentCommentId,
                        UserId = c.NguoiDungId ?? string.Empty,
                        UserName = c.ApplicationUser != null ? c.ApplicationUser.UserName! : "Unknown",
                        UserAvatar = c.ApplicationUser != null ? c.ApplicationUser.ProfilePicture : null,
                        Replies = _context.BinhLuan
                            .Include(r => r.ApplicationUser)
                            .Where(r => r.ParentCommentId == c.Id)
                            .OrderBy(r => r.NgayTao)
                            .Select(r => new CommentDto
                            {
                                Id = r.Id,
                                NoiDung = r.NoiDung ?? string.Empty,
                                NgayTao = r.NgayTao,
                                ParentCommentId = r.ParentCommentId,
                                UserId = r.NguoiDungId ?? string.Empty,
                                UserName = r.ApplicationUser != null ? r.ApplicationUser.UserName! : "Unknown",
                                UserAvatar = r.ApplicationUser != null ? r.ApplicationUser.ProfilePicture : null
                            }).ToList()
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<CommentDto>>
                {
                    Success = true,
                    Message = "Lấy danh sách comment thành công",
                    Data = comments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<CommentDto>>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách comment",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Thêm comment vào bài viết
        /// </summary>
        /// <param name="id">ID của bài viết</param>
        /// <param name="model">Nội dung comment</param>
        /// <returns>Comment vừa tạo</returns>
        [HttpPost("{id}/comments")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CommentDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<CommentDto>>> AddComment(Guid id, [FromBody] CreateCommentDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<CommentDto>
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ"
                    });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse<CommentDto>
                    {
                        Success = false,
                        Message = "Bạn cần đăng nhập để comment"
                    });
                }

                var post = await _context.BaiDang.FindAsync(id);
                if (post == null)
                {
                    return NotFound(new ApiResponse<CommentDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy bài viết"
                    });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Unauthorized(new ApiResponse<CommentDto>
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin người dùng"
                    });
                }

                var newComment = new BinhLuan
                {
                    Id = Guid.NewGuid(),
                    BaiDangId = id,
                    NguoiDungId = userId,
                    NguoiBinhLuan = user.UserName,
                    NoiDung = model.NoiDung,
                    NgayTao = DateTime.UtcNow,
                    ParentCommentId = model.ParentCommentId
                };

                _context.BinhLuan.Add(newComment);

                // Cập nhật số lượng comment
                post.SoBinhLuan = (post.SoBinhLuan ?? 0) + 1;

                await _context.SaveChangesAsync();

                var commentDto = new CommentDto
                {
                    Id = newComment.Id,
                    NoiDung = newComment.NoiDung ?? string.Empty,
                    NgayTao = newComment.NgayTao,
                    ParentCommentId = newComment.ParentCommentId,
                    UserId = userId,
                    UserName = user.UserName!,
                    UserAvatar = user.ProfilePicture
                };

                return CreatedAtAction(nameof(GetComments), new { id }, new ApiResponse<CommentDto>
                {
                    Success = true,
                    Message = "Thêm comment thành công",
                    Data = commentDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CommentDto>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi thêm comment",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Xóa bài viết (chỉ người tạo mới được xóa)
        /// </summary>
        /// <param name="id">ID của bài viết</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeletePost(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Bạn cần đăng nhập để xóa bài viết"
                    });
                }

                var post = await _context.BaiDang.FindAsync(id);
                if (post == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy bài viết"
                    });
                }

                // Kiểm tra quyền sở hữu
                if (post.NguoiDungId != userId)
                {
                    return StatusCode(403, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Bạn không có quyền xóa bài viết này"
                    });
                }

                _context.BaiDang.Remove(post);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Xóa bài viết thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi xóa bài viết",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}
