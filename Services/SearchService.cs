using System.Text.RegularExpressions;
using Hotel_API.Data;
using Hotel_API.Models;
using Hotel_API.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Hotel_API.Services
{
    /// <summary>
    /// Service để xử lý tìm kiếm tổng quát và riêng từng loại
    /// </summary>
    public class SearchService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SearchService> _logger;
        private readonly MediaUrlService _mediaUrlService;

        public SearchService(AppDbContext context, ILogger<SearchService> logger, MediaUrlService mediaUrlService)
        {
            _context = context;
            _logger = logger;
            _mediaUrlService = mediaUrlService;
        }

        /// <summary>
        /// Tìm kiếm tổng quát trên tất cả loại dữ liệu
        /// </summary>
        public async Task<GeneralSearchResponseDto> SearchGeneralAsync(SearchRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return new GeneralSearchResponseDto
                    {
                        Success = false,
                        Message = "Query không được để trống",
                        Data = new SearchDataDto(),
                        Pagination = new PaginationDto { Page = request.Page, Limit = request.Limit, Total = 0, TotalPages = 0 }
                    };
                }

                var query = request.Query.ToLower();
                var skip = (request.Page - 1) * request.Limit;

                var data = new SearchDataDto();

                // Tìm kiếm người dùng
                if (request.Type == "all" || request.Type == "users")
                {
                    data.Users = await SearchUsersAsync(query, request.Limit, skip);
                }

                // Tìm kiếm bài đăng
                if (request.Type == "all" || request.Type == "posts")
                {
                    data.Posts = await SearchPostsAsync(query, request.Limit, skip);
                }

                // Tìm kiếm bài thuốc
                if (request.Type == "all" || request.Type == "medicines")
                {
                    data.Medicines = await SearchMedicinesAsync(query, request.Limit, skip);
                }

                // Tìm kiếm món ăn
                if (request.Type == "all" || request.Type == "dishes")
                {
                    data.Dishes = await SearchDishesAsync(query, request.Limit, skip);
                }

                var totalResults = data.Users.Count + data.Posts.Count + data.Medicines.Count + data.Dishes.Count;

                return new GeneralSearchResponseDto
                {
                    Success = true,
                    Message = "Tìm kiếm thành công",
                    Data = data,
                    Pagination = new PaginationDto
                    {
                        Page = request.Page,
                        Limit = request.Limit,
                        Total = totalResults,
                        TotalPages = (int)Math.Ceiling((double)totalResults / request.Limit)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm tổng quát");
                return new GeneralSearchResponseDto
                {
                    Success = false,
                    Message = "Lỗi server: không thể thực hiện tìm kiếm"
                };
            }
        }

        /// <summary>
        /// Tìm kiếm người dùng
        /// </summary>
        private async Task<List<UserSearchResultDto>> SearchUsersAsync(string query, int limit, int skip)
        {
            try
            {
                var users = await _context.Users
                    .Where(u => (u.displayName != null && EF.Functions.Like(u.displayName.ToLower(), $"%{query}%")) ||
                                EF.Functions.Like(u.UserName!.ToLower(), $"%{query}%") ||
                                EF.Functions.Like(u.Email!.ToLower(), $"%{query}%"))
                    .Skip(skip)
                    .Take(limit)
                    .Select(u => new UserSearchResultDto
                    {
                        Id = u.Id,
                        Name = u.UserName,
                        Email = u.Email,
                        Avatar = u.ProfilePicture ?? string.Empty,
                        DisplayName = u.displayName ?? u.UserName
                    })
                    .ToListAsync();

                // Convert avatar URLs to full URLs
                foreach (var user in users)
                {
                    if (!string.IsNullOrEmpty(user.Avatar))
                    {
                        user.Avatar = _mediaUrlService.GetFullMediaUrl(user.Avatar);
                    }
                }

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm người dùng");
                return new List<UserSearchResultDto>();
            }
        }

        /// <summary>
        /// Tìm kiếm bài đăng
        /// </summary>
        private async Task<List<PostSearchResultDto>> SearchPostsAsync(string query, int limit, int skip)
        {
            try
            {
                return await _context.BaiDang
                    .Where(p => EF.Functions.Like(p.NoiDung!.ToLower(), $"%{query}%") ||
                                EF.Functions.Like(p.hashtags!.ToLower(), $"%{query}%") ||
                                EF.Functions.Like(p.keywords!.ToLower(), $"%{query}%"))
                    .Skip(skip)
                    .Take(limit)
                    .Select(p => new PostSearchResultDto
                    {
                        Id = p.Id,
                        Title = p.NoiDung!.Length > 100 ? p.NoiDung.Substring(0, 100) + "..." : p.NoiDung,
                        Content = p.NoiDung,
                        Image = p.DuongDanMedia,
                        UserId = p.NguoiDungId,
                        UserName = p.NguoiDang,
                        CreatedAt = p.NgayDang,
                        ViewCount = 0,
                        LikeCount = p.LuotThich
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm bài đăng");
                return new List<PostSearchResultDto>();
            }
        }

        /// <summary>
        /// Tìm kiếm bài thuốc
        /// </summary>
        private async Task<List<MedicineSearchResultDto>> SearchMedicinesAsync(string query, int limit, int skip)
        {
            try
            {
                return await _context.BaiThuocs
                    .Where(m => EF.Functions.Like(m.Ten.ToLower(), $"%{query}%") ||
                                EF.Functions.Like(m.MoTa!.ToLower(), $"%{query}%"))
                    .Where(m => m.TrangThai == 1)
                    .Skip(skip)
                    .Take(limit)
                    .Select(m => new MedicineSearchResultDto
                    {
                        Id = m.Id,
                        Name = m.Ten,
                        Description = m.MoTa,
                        Image = m.Image,
                        ViewCount = m.SoLuotXem,
                        LikeCount = m.SoLuotThich,
                        CreatedAt = m.NgayTao
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm bài thuốc");
                return new List<MedicineSearchResultDto>();
            }
        }

        /// <summary>
        /// Tìm kiếm món ăn
        /// </summary>
        private async Task<List<DishSearchResultDto>> SearchDishesAsync(string query, int limit, int skip)
        {
            try
            {
                return await _context.MonAns
                    .Where(d => EF.Functions.Like(d.Ten.ToLower(), $"%{query}%") ||
                                EF.Functions.Like(d.MoTa!.ToLower(), $"%{query}%") ||
                                EF.Functions.Like(d.Loai!.ToLower(), $"%{query}%"))
                    .Skip(skip)
                    .Take(limit)
                    .Select(d => new DishSearchResultDto
                    {
                        Id = d.Id,
                        Name = d.Ten,
                        Description = d.MoTa,
                        Image = d.Image,
                        Price = d.Gia,
                        Category = d.Loai,
                        Servings = d.SoNguoi,
                        ViewCount = d.LuotXem
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm món ăn");
                return new List<DishSearchResultDto>();
            }
        }

        /// <summary>
        /// Lấy gợi ý từ khóa
        /// </summary>
        public async Task<SuggestionsResponseDto> GetSuggestionsAsync(string query, string type = "all", int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return new SuggestionsResponseDto
                    {
                        Success = false,
                        Message = "Query không được để trống",
                        Data = new SuggestionsDataDto()
                    };
                }

                var q = query.ToLower();
                var suggestions = new HashSet<string>();

                // Lấy gợi ý từ người dùng
                if (type == "all" || type == "users")
                {
                    var userSuggestions = await _context.Users
                        .Where(u => u.displayName != null && EF.Functions.Like(u.displayName.ToLower(), $"%{q}%"))
                        .Select(u => u.displayName!)
                        .Take(limit)
                        .ToListAsync();
                    foreach (var s in userSuggestions)
                        suggestions.Add(s);
                }

                // Lấy gợi ý từ bài đăng
                if (type == "all" || type == "posts")
                {
                    var postSuggestions = await _context.BaiDang
                        .Where(p => EF.Functions.Like(p.NoiDung!.ToLower(), $"%{q}%"))
                        .Select(p => p.NoiDung!)
                        .Take(limit)
                        .ToListAsync();
                    foreach (var s in postSuggestions)
                        suggestions.Add(s);
                }

                // Lấy gợi ý từ bài thuốc
                if (type == "all" || type == "medicines")
                {
                    var medicineSuggestions = await _context.BaiThuocs
                        .Where(m => EF.Functions.Like(m.Ten.ToLower(), $"%{q}%"))
                        .Select(m => m.Ten)
                        .Take(limit)
                        .ToListAsync();
                    foreach (var s in medicineSuggestions)
                        suggestions.Add(s);
                }

                // Lấy gợi ý từ món ăn
                if (type == "all" || type == "dishes")
                {
                    var dishSuggestions = await _context.MonAns
                        .Where(d => EF.Functions.Like(d.Ten.ToLower(), $"%{q}%"))
                        .Select(d => d.Ten)
                        .Take(limit)
                        .ToListAsync();
                    foreach (var s in dishSuggestions)
                        suggestions.Add(s);
                }

                return new SuggestionsResponseDto
                {
                    Success = true,
                    Message = "Lấy gợi ý thành công",
                    Data = new SuggestionsDataDto
                    {
                        Suggestions = suggestions.Take(limit).ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy gợi ý tìm kiếm");
                return new SuggestionsResponseDto
                {
                    Success = false,
                    Message = "Lỗi server: không thể lấy gợi ý"
                };
            }
        }
    }
}
