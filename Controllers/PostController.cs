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
        /// Lấy home feed với thuật toán mix content
        /// </summary>
        [HttpGet("feed")]
        [ProducesResponseType(typeof(List<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<object>>> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var feedItems = new List<object>();

                // Mix content theo thuật toán:
                // 2 Friend Posts + 2 Friend BaiThuoc + 3 Top BaiThuoc + 2 Random Posts + 1 Random BaiThuoc

                if (!string.IsNullOrEmpty(userId))
                {
                    // TODO: Get friend posts when Friendship table is implemented
                    // For now, get recent posts
                    var recentPosts = await _context.BaiDang
                        .Include(p => p.ApplicationUser)
                        .Where(p => p.DaDuyet == true || p.DaDuyet == null)
                        .OrderByDescending(p => p.NgayDang)
                        .Take(4)
                        .Select(p => new
                        {
                            p.Id,
                            Type = "Post",
                            Content = p.NoiDung,
                            ImageUrl = p.DuongDanMedia,
                            NgayDang = p.NgayDang,
                            SoBinhLuan = p.SoBinhLuan,
                            SoChiaSe = p.so_chia_se,
                            LuotThich = p.LuotThich,
                            IsLiked = false, // TODO: Check if user liked
                            AuthorId = p.NguoiDungId,
                            AuthorName = p.ApplicationUser != null ? p.ApplicationUser.UserName : "Unknown",
                            Avartar = p.ApplicationUser != null ? p.ApplicationUser.ProfilePicture : null
                        })
                        .ToListAsync();

                    feedItems.AddRange(recentPosts.Select(p => new
                    {
                        p.Id,
                        p.Type,
                        p.Content,
                        ImageUrl = _mediaUrlService.GetFullMediaUrl(p.ImageUrl),
                        p.NgayDang,
                        p.SoBinhLuan,
                        p.SoChiaSe,
                        p.LuotThich,
                        p.IsLiked,
                        p.AuthorId,
                        p.AuthorName,
                        Avartar = _mediaUrlService.GetFullMediaUrl(p.Avartar)
                    }));
                }

                // Get top BaiThuoc by views
                var topBaiThuoc = await _context.BaiThuocs
                    .Include(b => b.ApplicationUser)
                    .Where(b => b.TrangThai == 1)
                    .OrderByDescending(b => b.SoLuotXem)
                    .Take(3)
                    .Select(b => new
                    {
                        b.Id,
                        Type = "BaiThuoc",
                        Content = b.MoTa,
                        ImageUrl = b.Image,
                        NgayDang = b.NgayTao,
                        SoBinhLuan = 0,
                        SoChiaSe = 0,
                        LuotThich = b.SoLuotThich ?? 0,
                        IsLiked = false,
                        AuthorId = b.NguoiDungId,
                        AuthorName = b.ApplicationUser != null ? b.ApplicationUser.UserName : "Unknown",
                        Avartar = b.ApplicationUser != null ? b.ApplicationUser.ProfilePicture : null,
                        TieuDe = b.Ten
                    })
                    .ToListAsync();

                feedItems.AddRange(topBaiThuoc.Select(b => new
                {
                    b.Id,
                    b.Type,
                    b.Content,
                    ImageUrl = _mediaUrlService.GetFullMediaUrl(b.ImageUrl),
                    b.NgayDang,
                    b.SoBinhLuan,
                    b.SoChiaSe,
                    b.LuotThich,
                    b.IsLiked,
                    b.AuthorId,
                    b.AuthorName,
                    Avartar = _mediaUrlService.GetFullMediaUrl(b.Avartar),
                    b.TieuDe
                }));

                // Get random posts
                var randomPosts = await _context.BaiDang
                    .Include(p => p.ApplicationUser)
                    .Where(p => p.DaDuyet == true || p.DaDuyet == null)
                    .OrderBy(x => Guid.NewGuid())
                    .Take(3)
                    .Select(p => new
                    {
                        p.Id,
                        Type = "Post",
                        Content = p.NoiDung,
                        ImageUrl = p.DuongDanMedia,
                        NgayDang = p.NgayDang,
                        SoBinhLuan = p.SoBinhLuan,
                        SoChiaSe = p.so_chia_se,
                        LuotThich = p.LuotThich,
                        IsLiked = false,
                        AuthorId = p.NguoiDungId,
                        AuthorName = p.ApplicationUser != null ? p.ApplicationUser.UserName : "Unknown",
                        Avartar = p.ApplicationUser != null ? p.ApplicationUser.ProfilePicture : null
                    })
                    .ToListAsync();

                feedItems.AddRange(randomPosts.Select(p => new
                {
                    p.Id,
                    p.Type,
                    p.Content,
                    ImageUrl = _mediaUrlService.GetFullMediaUrl(p.ImageUrl),
                    p.NgayDang,
                    p.SoBinhLuan,
                    p.SoChiaSe,
                    p.LuotThich,
                    p.IsLiked,
                    p.AuthorId,
                    p.AuthorName,
                    Avartar = _mediaUrlService.GetFullMediaUrl(p.Avartar)
                }));

                // Shuffle and paginate
                var shuffled = feedItems.OrderBy(x => Guid.NewGuid()).ToList();
                var paginatedFeed = shuffled.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                return Ok(paginatedFeed);
            }
            catch (Exception)
            {
                return StatusCode(500, new List<object>());
            }
        }

        /// <summary>
        /// Lấy chi tiết bài viết hoặc bài thuốc
        /// </summary>
        [HttpGet("detail")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> GetDetail([FromQuery] Guid id, [FromQuery] string? type = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Try to detect type if not provided
                if (string.IsNullOrEmpty(type))
                {
                    var isPost = await _context.BaiDang.AnyAsync(p => p.Id == id);
                    type = isPost ? "Post" : "BaiThuoc";
                }

                if (type.Equals("Post", StringComparison.OrdinalIgnoreCase))
                {
                    var post = await _context.BaiDang
                        .Include(p => p.ApplicationUser)
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (post == null)
                    {
                        return NotFound(new { message = "Không tìm thấy bài viết." });
                    }

                    var isLiked = userId != null && await _context.BaiDang_LuotThich
                        .AnyAsync(l => l.baidang_id == id && l.nguoidung_id == userId);

                    return Ok(new
                    {
                        post.Id,
                        Content = post.NoiDung,
                        ImageUrl = _mediaUrlService.GetFullMediaUrl(post.DuongDanMedia),
                        NgayDang = post.NgayDang,
                        SoBinhLuan = post.SoBinhLuan ?? 0,
                        SoChiaSe = post.so_chia_se,
                        LuotThich = post.LuotThich ?? 0,
                        IsLiked = isLiked,
                        AuthorId = post.NguoiDungId,
                        AuthorName = post.ApplicationUser?.UserName,
                        Avartar = _mediaUrlService.GetFullMediaUrl(post.ApplicationUser?.ProfilePicture)
                    });
                }
                else if (type.Equals("BaiThuoc", StringComparison.OrdinalIgnoreCase))
                {
                    var baiThuoc = await _context.BaiThuocs
                        .Include(b => b.ApplicationUser)
                        .FirstOrDefaultAsync(b => b.Id == id);

                    if (baiThuoc == null)
                    {
                        return NotFound(new { message = "Không tìm thấy bài viết." });
                    }

                    // Increment view count
                    baiThuoc.SoLuotXem = (baiThuoc.SoLuotXem ?? 0) + 1;
                    await _context.SaveChangesAsync();

                    // TODO: Check if user liked
                    var isLiked = false;

                    return Ok(new
                    {
                        baiThuoc.Id,
                        TieuDe = baiThuoc.Ten,
                        MoTa = baiThuoc.MoTa,
                        ImageUrl = _mediaUrlService.GetFullMediaUrl(baiThuoc.Image),
                        NgayTao = baiThuoc.NgayTao,
                        SoLuotThich = baiThuoc.SoLuotThich ?? 0,
                        IsLiked = isLiked,
                        AuthorId = baiThuoc.NguoiDungId,
                        AuthorName = baiThuoc.ApplicationUser?.UserName,
                        Avartar = _mediaUrlService.GetFullMediaUrl(baiThuoc.ApplicationUser?.ProfilePicture),
                        NguyenLieu = (string?)null,  // TODO: Chưa implement
                        HuongDan = (string?)null,    // TODO: Chưa implement
                        CongDung = (string?)null     // TODO: Chưa implement
                    });
                }
                else
                {
                    return BadRequest(new { message = "Type không hợp lệ. Chỉ chấp nhận 'Post' hoặc 'BaiThuoc'." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
            }
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
        /// Upload ảnh cho bài viết dưới dạng Base64
        /// </summary>
        /// <param name="model">DTO chứa Base64 string của ảnh</param>
        /// <returns>Base64 string của ảnh đã upload</returns>
        [HttpPost("upload")]
        [Authorize]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<object>> UploadImage([FromBody] ImageUploadDto model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Bạn cần đăng nhập để upload ảnh"
                    });
                }

                if (string.IsNullOrEmpty(model.ImageBase64) || string.IsNullOrEmpty(model.FileName))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "ImageBase64 và FileName không được để trống"
                    });
                }

                // Xác thực Base64
                var base64Data = Base64ImageService.ExtractBase64(model.ImageBase64);

                if (!Base64ImageService.IsValidImageBase64(base64Data, model.FileName))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Ảnh không hợp lệ. Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif, webp) với kích thước tối đa 5MB"
                    });
                }

                // Tạo Data URL cho ảnh
                var dataUrl = Base64ImageService.CreateDataUrl(base64Data, model.FileName);
                var fileSize = Base64ImageService.GetBase64Size(base64Data);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Upload ảnh thành công",
                    Data = new
                    {
                        fileName = model.FileName,
                        imageBase64 = dataUrl,
                        size = fileSize,
                        mimeType = Base64ImageService.GetMimeType(model.FileName)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi upload ảnh",
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

        /// <summary>
        /// Lấy danh sách bài viết được người dùng hiện tại yêu thích (Infinity Scroll - Offset based)
        /// </summary>
        /// <param name="offset">Số bài viết đã skip (mặc định 0)</param>
        /// <param name="limit">Số lượng bài viết trả về (mặc định 10, max 50)</param>
        [HttpGet("user/likes")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PostInfinityResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<PostInfinityResult>>> GetUserLikedPosts(
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 10)
        {
            try
            {
                if (offset < 0) offset = 0;
                if (limit < 1) limit = 10;
                if (limit > 50) limit = 100;

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse<PostInfinityResult>
                    {
                        Success = false,
                        Message = "Bạn cần đăng nhập để xem bài viết yêu thích"
                    });
                }

                // Lấy danh sách bài viết được yêu thích
                var likedPosts = await _context.BaiDang_LuotThich
                    .Include(l => l.BaiDang)
                    .ThenInclude(p => p!.ApplicationUser)
                    .Where(l => l.nguoidung_id == userId)
                    .OrderByDescending(l => l.ngay_thich)
                    .Skip(offset)
                    .Take(limit + 1)
                    .ToListAsync();

                var hasMore = likedPosts.Count > limit;
                if (hasMore)
                {
                    likedPosts = likedPosts.Take(limit).ToList();
                }

                var postDtos = new List<PostDto>();
                foreach (var like in likedPosts)
                {
                    if (like.BaiDang != null)
                    {
                        var p = like.BaiDang;
                        postDtos.Add(new PostDto
                        {
                            Id = p.Id,
                            NoiDung = p.NoiDung ?? "",
                            Loai = p.Loai,
                            DuongDanMedia = _mediaUrlService.GetFullMediaUrl(p.DuongDanMedia),
                            NgayDang = p.NgayDang,
                            LuotThich = p.LuotThich ?? 0,
                            SoBinhLuan = p.SoBinhLuan ?? 0,
                            SoChiaSe = p.so_chia_se,
                            IsLiked = true,
                            Hashtags = p.hashtags,
                            AuthorId = p.NguoiDungId ?? "",
                            AuthorName = p.ApplicationUser?.UserName ?? "Unknown",
                            AuthorAvatar = _mediaUrlService.GetFullMediaUrl(p.ApplicationUser?.ProfilePicture)
                        });
                    }
                }

                return Ok(new ApiResponse<PostInfinityResult>
                {
                    Success = true,
                    Message = "Danh sách bài viết yêu thích",
                    Data = new PostInfinityResult
                    {
                        Posts = postDtos,
                        HasMore = hasMore
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PostInfinityResult>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách bài viết yêu thích",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Lấy danh sách bài viết do người dùng hiện tại tạo (Infinity Scroll - Offset based)
        /// </summary>
        /// <param name="offset">Số bài viết đã skip (mặc định 0)</param>
        /// <param name="limit">Số lượng bài viết trả về (mặc định 10, max 50)</param>
        [HttpGet("user/posts")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PostInfinityResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<PostInfinityResult>>> GetUserPosts(
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
                    return Unauthorized(new ApiResponse<PostInfinityResult>
                    {
                        Success = false,
                        Message = "Bạn cần đăng nhập để xem bài viết của mình"
                    });
                }

                var userPosts = await _context.BaiDang
                    .Where(p => p.NguoiDungId == userId)
                    .Include(p => p.ApplicationUser)
                    .OrderByDescending(p => p.NgayDang)
                    .Skip(offset)
                    .Take(limit + 1)
                    .ToListAsync();

                var hasMore = userPosts.Count > limit;
                if (hasMore)
                {
                    userPosts = userPosts.Take(limit).ToList();
                }

                var likedPostIds = await _context.BaiDang_LuotThich
                    .Where(l => l.nguoidung_id == userId)
                    .Select(l => l.baidang_id)
                    .ToListAsync();

                var postDtos = userPosts.Select(p => new PostDto
                {
                    Id = p.Id,
                    NoiDung = p.NoiDung ?? "",
                    Loai = p.Loai,
                    DuongDanMedia = _mediaUrlService.GetFullMediaUrl(p.DuongDanMedia),
                    NgayDang = p.NgayDang,
                    LuotThich = p.LuotThich ?? 0,
                    SoBinhLuan = p.SoBinhLuan ?? 0,
                    SoChiaSe = p.so_chia_se,
                    IsLiked = likedPostIds.Contains(p.Id),
                    Hashtags = p.hashtags,
                    AuthorId = p.NguoiDungId ?? "",
                    AuthorName = p.ApplicationUser?.UserName ?? "Unknown",
                    AuthorAvatar = _mediaUrlService.GetFullMediaUrl(p.ApplicationUser?.ProfilePicture)
                }).ToList();

                return Ok(new ApiResponse<PostInfinityResult>
                {
                    Success = true,
                    Message = "Danh sách bài viết của bạn",
                    Data = new PostInfinityResult
                    {
                        Posts = postDtos,
                        HasMore = hasMore
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PostInfinityResult>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách bài viết",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Lấy danh sách bình luận do người dùng hiện tại tạo (Infinity Scroll - Offset based)
        /// </summary>
        /// <param name="offset">Số bình luận đã skip (mặc định 0)</param>
        /// <param name="limit">Số lượng bình luận trả về (mặc định 10, max 50)</param>
        [HttpGet("user/comments")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CommentInfinityResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<CommentInfinityResult>>> GetUserComments(
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
                    return Unauthorized(new ApiResponse<CommentInfinityResult>
                    {
                        Success = false,
                        Message = "Bạn cần đăng nhập để xem bình luận của mình"
                    });
                }

                var userComments = await _context.BinhLuan
                    .Where(c => c.NguoiDungId == userId && c.ParentCommentId == null)
                    .Include(c => c.ApplicationUser)
                    .OrderByDescending(c => c.NgayTao)
                    .Skip(offset)
                    .Take(limit + 1)
                    .ToListAsync();

                var hasMore = userComments.Count > limit;
                if (hasMore)
                {
                    userComments = userComments.Take(limit).ToList();
                }

                var commentDtos = userComments.Select(c => new CommentDto
                {
                    Id = c.Id,
                    NoiDung = c.NoiDung ?? "",
                    NgayTao = c.NgayTao,
                    ParentCommentId = c.ParentCommentId,
                    UserId = c.NguoiDungId ?? "",
                    UserName = c.ApplicationUser?.UserName ?? "Unknown",
                    UserAvatar = _mediaUrlService.GetFullMediaUrl(c.ApplicationUser?.ProfilePicture),
                    Replies = new List<CommentDto>()
                }).ToList();

                return Ok(new ApiResponse<CommentInfinityResult>
                {
                    Success = true,
                    Message = "Danh sách bình luận của bạn",
                    Data = new CommentInfinityResult
                    {
                        Comments = commentDtos,
                        HasMore = hasMore
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CommentInfinityResult>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách bình luận",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Lấy danh sách bài viết của một người dùng bất kỳ (Public - Infinity Scroll - Offset based)
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <param name="offset">Số bài viết đã skip (mặc định 0)</param>
        /// <param name="limit">Số lượng bài viết trả về (mặc định 10, max 50)</param>
        [HttpGet("public/{userId}/posts")]
        [ProducesResponseType(typeof(ApiResponse<PostInfinityResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PostInfinityResult>>> GetUserPostsByUserId(
            string userId,
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 100)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new ApiResponse<PostInfinityResult>
                    {
                        Success = false,
                        Message = "ID người dùng không được để trống"
                    });
                }

                if (offset < 0) offset = 0;
                if (limit < 1) limit = 10;
                if (limit > 50) limit = 100;

                var userPosts = await _context.BaiDang
                    .Where(p => p.NguoiDungId == userId)
                    .Include(p => p.ApplicationUser)
                    .OrderByDescending(p => p.NgayDang)
                    .Skip(offset)
                    .Take(limit + 1)
                    .ToListAsync();

                var hasMore = userPosts.Count > limit;
                if (hasMore)
                {
                    userPosts = userPosts.Take(limit).ToList();
                }

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var likedPostIds = new List<Guid>();

                if (!string.IsNullOrEmpty(currentUserId))
                {
                    likedPostIds = await _context.BaiDang_LuotThich
                        .Where(l => l.nguoidung_id == currentUserId)
                        .Select(l => l.baidang_id)
                        .ToListAsync();
                }

                var postDtos = userPosts.Select(p => new PostDto
                {
                    Id = p.Id,
                    NoiDung = p.NoiDung ?? "",
                    Loai = p.Loai,
                    DuongDanMedia = _mediaUrlService.GetFullMediaUrl(p.DuongDanMedia),
                    NgayDang = p.NgayDang,
                    LuotThich = p.LuotThich ?? 0,
                    SoBinhLuan = p.SoBinhLuan ?? 0,
                    SoChiaSe = p.so_chia_se,
                    IsLiked = likedPostIds.Contains(p.Id),
                    Hashtags = p.hashtags,
                    AuthorId = p.NguoiDungId ?? "",
                    AuthorName = p.ApplicationUser?.UserName ?? "Unknown",
                    AuthorAvatar = _mediaUrlService.GetFullMediaUrl(p.ApplicationUser?.ProfilePicture)
                }).ToList();

                return Ok(new ApiResponse<PostInfinityResult>
                {
                    Success = true,
                    Message = "Danh sách bài viết của người dùng",
                    Data = new PostInfinityResult
                    {
                        Posts = postDtos,
                        HasMore = hasMore
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PostInfinityResult>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách bài viết",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Lấy danh sách bình luận của một người dùng bất kỳ (Public - Infinity Scroll - Offset based)
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <param name="offset">Số bình luận đã skip (mặc định 0)</param>
        /// <param name="limit">Số lượng bình luận trả về (mặc định 10, max 50)</param>
        [HttpGet("public/{userId}/comments")]
        [ProducesResponseType(typeof(ApiResponse<CommentInfinityResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<CommentInfinityResult>>> GetUserCommentsByUserId(
            string userId,
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new ApiResponse<CommentInfinityResult>
                    {
                        Success = false,
                        Message = "ID người dùng không được để trống"
                    });
                }

                if (offset < 0) offset = 0;
                if (limit < 1) limit = 10;
                if (limit > 50) limit = 100;

                var userComments = await _context.BinhLuan
                    .Where(c => c.NguoiDungId == userId && c.ParentCommentId == null)
                    .Include(c => c.ApplicationUser)
                    .OrderByDescending(c => c.NgayTao)
                    .Skip(offset)
                    .Take(limit + 1)
                    .ToListAsync();

                var hasMore = userComments.Count > limit;
                if (hasMore)
                {
                    userComments = userComments.Take(limit).ToList();
                }

                var commentDtos = userComments.Select(c => new CommentDto
                {
                    Id = c.Id,
                    NoiDung = c.NoiDung ?? "",
                    NgayTao = c.NgayTao,
                    ParentCommentId = c.ParentCommentId,
                    UserId = c.NguoiDungId ?? "",
                    UserName = c.ApplicationUser?.UserName ?? "Unknown",
                    UserAvatar = _mediaUrlService.GetFullMediaUrl(c.ApplicationUser?.ProfilePicture),
                    Replies = new List<CommentDto>()
                }).ToList();

                return Ok(new ApiResponse<CommentInfinityResult>
                {
                    Success = true,
                    Message = "Danh sách bình luận của người dùng",
                    Data = new CommentInfinityResult
                    {
                        Comments = commentDtos,
                        HasMore = hasMore
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<CommentInfinityResult>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách bình luận",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}
