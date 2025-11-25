using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Hotel_API.Models.ViewModels;
using Hotel_API.Services;

namespace Hotel_API.Controllers
{
    /// <summary>
    /// Controller xử lý tìm kiếm tổng quát và riêng từng loại
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly SearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(SearchService searchService, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        /// <summary>
        /// Tìm kiếm tổng quát trên tất cả loại dữ liệu
        /// </summary>
        /// <remarks>
        /// Endpoint này cho phép tìm kiếm trên người dùng, bài đăng, bài thuốc và món ăn.
        /// Hỗ trợ tìm kiếm không dấu (ví dụ: "com ga" sẽ tìm được "cơm gà").
        /// 
        /// Các giá trị type:
        /// - all (mặc định): Tìm kiếm tất cả loại dữ liệu
        /// - users: Chỉ tìm người dùng
        /// - posts: Chỉ tìm bài đăng
        /// - medicines: Chỉ tìm bài thuốc
        /// - dishes: Chỉ tìm món ăn
        /// </remarks>
        /// <param name="request">Thông tin tìm kiếm (query, type, page, limit)</param>
        /// <returns>Danh sách kết quả tìm kiếm</returns>
        [HttpGet]
        [ProducesResponseType(typeof(GeneralSearchResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GeneralSearchResponseDto>> Search([FromQuery] SearchRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length < 2)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Success = false,
                        Message = "Query phải từ 2 ký tự trở lên",
                        Code = "INVALID_QUERY"
                    });
                }

                var result = await _searchService.SearchGeneralAsync(request);

                if (!result.Success)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Success = false,
                        Message = result.Message,
                        Code = "SEARCH_ERROR"
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi tìm kiếm tổng quát");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseDto
                {
                    Success = false,
                    Message = "Lỗi server: không thể thực hiện tìm kiếm",
                    Code = "SERVER_ERROR"
                });
            }
        }

        /// <summary>
        /// Tìm kiếm người dùng
        /// </summary>
        [HttpGet("users")]
        [ProducesResponseType(typeof(TypedSearchResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TypedSearchResponseDto>> SearchUsers(
            [FromQuery][Required(ErrorMessage = "Query không được để trống")] string q,
            [FromQuery][Range(1, int.MaxValue)] int page = 1,
            [FromQuery][Range(1, 100)] int limit = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Success = false,
                        Message = "Query phải từ 2 ký tự trở lên",
                        Code = "INVALID_QUERY"
                    });
                }

                var request = new SearchRequestDto
                {
                    Query = q,
                    Type = "users",
                    Page = page,
                    Limit = limit
                };

                var result = await _searchService.SearchGeneralAsync(request);

                return Ok(new TypedSearchResponseDto
                {
                    Success = true,
                    Message = "Tìm kiếm người dùng thành công",
                    Data = new { users = result.Data?.Users },
                    Pagination = result.Pagination
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi tìm kiếm người dùng");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseDto
                {
                    Success = false,
                    Message = "Lỗi server: không thể tìm kiếm người dùng",
                    Code = "SERVER_ERROR"
                });
            }
        }

        /// <summary>
        /// Tìm kiếm bài đăng
        /// </summary>
        [HttpGet("posts")]
        [ProducesResponseType(typeof(TypedSearchResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TypedSearchResponseDto>> SearchPosts(
            [FromQuery][Required(ErrorMessage = "Query không được để trống")] string q,
            [FromQuery][Range(1, int.MaxValue)] int page = 1,
            [FromQuery][Range(1, 100)] int limit = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Success = false,
                        Message = "Query phải từ 2 ký tự trở lên",
                        Code = "INVALID_QUERY"
                    });
                }

                var request = new SearchRequestDto
                {
                    Query = q,
                    Type = "posts",
                    Page = page,
                    Limit = limit
                };

                var result = await _searchService.SearchGeneralAsync(request);

                return Ok(new TypedSearchResponseDto
                {
                    Success = true,
                    Message = "Tìm kiếm bài đăng thành công",
                    Data = new { posts = result.Data?.Posts },
                    Pagination = result.Pagination
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi tìm kiếm bài đăng");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseDto
                {
                    Success = false,
                    Message = "Lỗi server: không thể tìm kiếm bài đăng",
                    Code = "SERVER_ERROR"
                });
            }
        }

        /// <summary>
        /// Tìm kiếm bài thuốc (hỗ trợ không dấu)
        /// </summary>
        [HttpGet("medicines")]
        [ProducesResponseType(typeof(TypedSearchResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TypedSearchResponseDto>> SearchMedicines(
            [FromQuery][Required(ErrorMessage = "Query không được để trống")] string q,
            [FromQuery][Range(1, int.MaxValue)] int page = 1,
            [FromQuery][Range(1, 100)] int limit = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Success = false,
                        Message = "Query phải từ 2 ký tự trở lên",
                        Code = "INVALID_QUERY"
                    });
                }

                var request = new SearchRequestDto
                {
                    Query = q,
                    Type = "medicines",
                    Page = page,
                    Limit = limit
                };

                var result = await _searchService.SearchGeneralAsync(request);

                return Ok(new TypedSearchResponseDto
                {
                    Success = true,
                    Message = "Tìm kiếm bài thuốc thành công",
                    Data = new { medicines = result.Data?.Medicines },
                    Pagination = result.Pagination
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi tìm kiếm bài thuốc");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseDto
                {
                    Success = false,
                    Message = "Lỗi server: không thể tìm kiếm bài thuốc",
                    Code = "SERVER_ERROR"
                });
            }
        }

        /// <summary>
        /// Tìm kiếm món ăn (hỗ trợ không dấu)
        /// </summary>
        [HttpGet("dishes")]
        [ProducesResponseType(typeof(TypedSearchResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TypedSearchResponseDto>> SearchDishes(
            [FromQuery][Required(ErrorMessage = "Query không được để trống")] string q,
            [FromQuery][Range(1, int.MaxValue)] int page = 1,
            [FromQuery][Range(1, 100)] int limit = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Success = false,
                        Message = "Query phải từ 2 ký tự trở lên",
                        Code = "INVALID_QUERY"
                    });
                }

                var request = new SearchRequestDto
                {
                    Query = q,
                    Type = "dishes",
                    Page = page,
                    Limit = limit
                };

                var result = await _searchService.SearchGeneralAsync(request);

                return Ok(new TypedSearchResponseDto
                {
                    Success = true,
                    Message = "Tìm kiếm món ăn thành công",
                    Data = new { dishes = result.Data?.Dishes },
                    Pagination = result.Pagination
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi tìm kiếm món ăn");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseDto
                {
                    Success = false,
                    Message = "Lỗi server: không thể tìm kiếm món ăn",
                    Code = "SERVER_ERROR"
                });
            }
        }

        /// <summary>
        /// Lấy gợi ý tìm kiếm (Autocomplete)
        /// </summary>
        /// <remarks>
        /// Endpoint này cung cấp gợi ý từ khóa dựa trên query nhập vào.
        /// Hữu ích cho chức năng autocomplete trong UI.
        /// </remarks>
        [HttpGet("suggestions")]
        [ProducesResponseType(typeof(SuggestionsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SuggestionsResponseDto>> GetSuggestions(
            [FromQuery][Required(ErrorMessage = "Query không được để trống")] string q,
            [FromQuery] string type = "all",
            [FromQuery][Range(1, 50)] int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Success = false,
                        Message = "Query phải từ 2 ký tự trở lên",
                        Code = "INVALID_QUERY"
                    });
                }

                var result = await _searchService.GetSuggestionsAsync(q, type, limit);

                if (!result.Success)
                {
                    return BadRequest(new ErrorResponseDto
                    {
                        Success = false,
                        Message = result.Message,
                        Code = "SUGGESTIONS_ERROR"
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi lấy gợi ý tìm kiếm");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponseDto
                {
                    Success = false,
                    Message = "Lỗi server: không thể lấy gợi ý tìm kiếm",
                    Code = "SERVER_ERROR"
                });
            }
        }
    }
}
