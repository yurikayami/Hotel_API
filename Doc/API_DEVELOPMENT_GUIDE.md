# 🚀 Hướng Dẫn Phát Triển API - Hotel Web

> Tài liệu này hướng dẫn cách tạo API mới, best practices và kiến trúc code cho dự án Hotel Web.

---

## 📋 Mục Lục

1. [Kiến Trúc Dự Án](#1-kiến-trúc-dự-án)
2. [Cách Tạo API Controller Mới](#2-cách-tạo-api-controller-mới)
3. [Services Layer](#3-services-layer)
4. [Repository Pattern](#4-repository-pattern)
5. [Database Migrations](#5-database-migrations)
6. [Best Practices](#6-best-practices)
7. [Testing](#7-testing)

---

## 1. Kiến Trúc Dự Án

### 1.1 Project Structure

```
Hotel_Web/
├── Controllers/              # MVC & API Controllers
│   ├── *APIController.cs    # API cho Flutter
│   └── *Controller.cs       # MVC cho Web
├── Services/                 # Business Logic
│   ├── *Service.cs
│   └── I*Service.cs         # Interfaces
├── Repositories/             # Data Access Layer
│   ├── *Repository.cs
│   └── I*Repository.cs
├── Models/                   # Entity Models
│   ├── *.cs                 # Domain Models
│   └── ViewModels/          # DTOs
├── Data/
│   └── AppDbContext.cs      # EF Core DbContext
├── Migrations/               # EF Migrations
├── ChatHub/                  # SignalR Hubs
└── wwwroot/                  # Static files
    └── uploads/             # User uploaded files
```

### 1.2 Dependency Flow

```
Controller → Service → Repository → DbContext → Database
     ↓
  DTO/ViewModel
```

**Nguyên tắc**:
- Controller chỉ nhận request và trả response
- Service chứa business logic
- Repository chứa data access logic
- Models là domain entities

---

## 2. Cách Tạo API Controller Mới

### 2.1 Template Cơ Bản

Tạo file `Controllers/YourAPIController.cs`:

```csharp
using Hotel_Web.Data;
using Hotel_Web.Models;
using Hotel_Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class YourAPIController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IYourService _yourService;

        public YourAPIController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IYourService yourService)
        {
            _context = context;
            _userManager = userManager;
            _yourService = yourService;
        }

        // GET: api/YourAPI
        [HttpGet]
        public async Task<ActionResult<IEnumerable<YourDto>>> GetAll()
        {
            try
            {
                var items = await _yourService.GetAllAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    error = "Có lỗi xảy ra", 
                    details = ex.Message 
                });
            }
        }

        // GET: api/YourAPI/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<YourDto>> GetById(Guid id)
        {
            try
            {
                var item = await _yourService.GetByIdAsync(id);
                
                if (item == null)
                    return NotFound(new { message = "Không tìm thấy" });
                
                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST: api/YourAPI
        [HttpPost]
        [Authorize] // Yêu cầu đăng nhập
        public async Task<ActionResult<YourDto>> Create([FromBody] CreateYourDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = _userManager.GetUserId(User);
                var result = await _yourService.CreateAsync(dto, userId);
                
                return CreatedAtAction(
                    nameof(GetById), 
                    new { id = result.Id }, 
                    result
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // PUT: api/YourAPI/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<YourDto>> Update(Guid id, [FromBody] UpdateYourDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = _userManager.GetUserId(User);
                var result = await _yourService.UpdateAsync(id, dto, userId);
                
                if (result == null)
                    return NotFound(new { message = "Không tìm thấy" });
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // DELETE: api/YourAPI/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var success = await _yourService.DeleteAsync(id, userId);
                
                if (!success)
                    return NotFound(new { message = "Không tìm thấy" });
                
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
```

### 2.2 API với File Upload

```csharp
[HttpPost("upload")]
[Authorize]
public async Task<ActionResult> UploadWithFile(
    [FromForm] YourDto dto, 
    IFormFile file)
{
    if (file == null || file.Length == 0)
        return BadRequest("File is required");

    try
    {
        // 1. Validate file
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        
        if (!allowedExtensions.Contains(extension))
            return BadRequest("Invalid file type");

        if (file.Length > 5 * 1024 * 1024) // 5MB
            return BadRequest("File too large (max 5MB)");

        // 2. Save file
        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(), 
            "wwwroot", 
            "uploads", 
            "your-folder"
        );
        
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 3. Create public URL
        var fileUrl = $"/uploads/your-folder/{fileName}";
        dto.ImageUrl = fileUrl;

        // 4. Save to database
        var userId = _userManager.GetUserId(User);
        var result = await _yourService.CreateAsync(dto, userId);

        return Ok(result);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = ex.Message });
    }
}
```

### 2.3 API với Pagination

```csharp
[HttpGet("paged")]
public async Task<ActionResult<PagedResult<YourDto>>> GetPaged(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? search = null,
    [FromQuery] string? sortBy = "CreatedAt",
    [FromQuery] bool descending = true)
{
    try
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var result = await _yourService.GetPagedAsync(
            page, 
            pageSize, 
            search, 
            sortBy, 
            descending
        );

        return Ok(result);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = ex.Message });
    }
}

// DTO cho pagination
public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
```

---

## 3. Services Layer

### 3.1 Tạo Service Interface

File: `Services/IYourService.cs`

```csharp
namespace Hotel_Web.Services
{
    public interface IYourService
    {
        Task<IEnumerable<YourDto>> GetAllAsync();
        Task<YourDto?> GetByIdAsync(Guid id);
        Task<PagedResult<YourDto>> GetPagedAsync(
            int page, 
            int pageSize, 
            string? search, 
            string? sortBy, 
            bool descending
        );
        Task<YourDto> CreateAsync(CreateYourDto dto, string userId);
        Task<YourDto?> UpdateAsync(Guid id, UpdateYourDto dto, string userId);
        Task<bool> DeleteAsync(Guid id, string userId);
    }
}
```

### 3.2 Implement Service

File: `Services/YourService.cs`

```csharp
using Hotel_Web.Data;
using Hotel_Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Web.Services
{
    public class YourService : IYourService
    {
        private readonly AppDbContext _context;

        public YourService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<YourDto>> GetAllAsync()
        {
            return await _context.YourEntity
                .Select(e => new YourDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    CreatedAt = e.CreatedAt,
                    // Map other properties
                })
                .ToListAsync();
        }

        public async Task<YourDto?> GetByIdAsync(Guid id)
        {
            return await _context.YourEntity
                .Where(e => e.Id == id)
                .Select(e => new YourDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    // Map properties
                })
                .FirstOrDefaultAsync();
        }

        public async Task<YourDto> CreateAsync(CreateYourDto dto, string userId)
        {
            var entity = new YourEntity
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                // Map other properties
            };

            _context.YourEntity.Add(entity);
            await _context.SaveChangesAsync();

            return new YourDto
            {
                Id = entity.Id,
                Name = entity.Name,
                // Return mapped DTO
            };
        }

        public async Task<YourDto?> UpdateAsync(
            Guid id, 
            UpdateYourDto dto, 
            string userId)
        {
            var entity = await _context.YourEntity
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entity == null)
                return null;

            // Check ownership
            if (entity.UserId != userId)
                throw new UnauthorizedAccessException();

            // Update properties
            entity.Name = dto.Name;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new YourDto
            {
                Id = entity.Id,
                Name = entity.Name,
            };
        }

        public async Task<bool> DeleteAsync(Guid id, string userId)
        {
            var entity = await _context.YourEntity
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entity == null)
                return false;

            // Check ownership
            if (entity.UserId != userId)
                throw new UnauthorizedAccessException();

            _context.YourEntity.Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<PagedResult<YourDto>> GetPagedAsync(
            int page, 
            int pageSize, 
            string? search, 
            string? sortBy, 
            bool descending)
        {
            var query = _context.YourEntity.AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e => 
                    e.Name.Contains(search) || 
                    e.Description.Contains(search)
                );
            }

            // Total count
            var totalCount = await query.CountAsync();

            // Sort
            query = sortBy?.ToLower() switch
            {
                "name" => descending 
                    ? query.OrderByDescending(e => e.Name)
                    : query.OrderBy(e => e.Name),
                _ => descending
                    ? query.OrderByDescending(e => e.CreatedAt)
                    : query.OrderBy(e => e.CreatedAt)
            };

            // Paginate
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new YourDto
                {
                    Id = e.Id,
                    Name = e.Name,
                })
                .ToListAsync();

            return new PagedResult<YourDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
```

### 3.3 Register Service trong Program.cs

```csharp
// Program.cs
builder.Services.AddScoped<IYourService, YourService>();
```

---

## 4. Repository Pattern

### 4.1 Generic Repository Interface

File: `Repositories/IRepository.cs`

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
```

### 4.2 Implement Generic Repository

File: `Repositories/Repository.cs`

```csharp
using Hotel_Web.Data;
using Microsoft.EntityFrameworkCore;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.FindAsync(id) != null;
    }
}
```

### 4.3 Specific Repository

File: `Repositories/IOrderRepository.cs`

```csharp
public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<CartItemDto>> GetCartItems(string userId);
    Task<Order?> GetOrderWithDetails(Guid orderId);
}
```

File: `Repositories/OrderRepository.cs`

```csharp
using Hotel_Web.Data;
using Hotel_Web.Models;
using Microsoft.EntityFrameworkCore;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CartItemDto>> GetCartItems(string userId)
    {
        return await _context.GioHangChiTiet
            .Include(g => g.MonAn)
            .Include(g => g.GioHang)
            .Where(g => g.GioHang.NguoiDungId == userId)
            .Select(g => new CartItemDto
            {
                Id = g.Id,
                MonAnId = g.MonAnId,
                TenMonAn = g.MonAn.Ten,
                SoLuong = g.SoLuong,
                DonGia = g.MonAn.Gia,
                ThanhTien = g.ThanhTien,
                ImageUrl = g.MonAn.Image
            })
            .ToListAsync();
    }

    public async Task<Order?> GetOrderWithDetails(Guid orderId)
    {
        return await _context.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.MonAn)
            .FirstOrDefaultAsync(o => o.OrderKey == orderId);
    }
}
```

---

## 5. Database Migrations

### 5.1 Tạo Model mới

File: `Models/YourEntity.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Web.Models
{
    public class YourEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [ForeignKey("User")]
        public string? UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ApplicationUser? User { get; set; }
    }
}
```

### 5.2 Thêm vào DbContext

File: `Data/AppDbContext.cs`

```csharp
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    // ... existing DbSets ...

    public DbSet<YourEntity> YourEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships nếu cần
        modelBuilder.Entity<YourEntity>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### 5.3 Tạo Migration

```powershell
# Trong Package Manager Console
Add-Migration AddYourEntity

# Hoặc dùng dotnet CLI
dotnet ef migrations add AddYourEntity

# Apply migration
Update-Database
# hoặc
dotnet ef database update
```

### 5.4 Seed Data (Optional)

```csharp
// Trong OnModelCreating của AppDbContext
modelBuilder.Entity<YourEntity>().HasData(
    new YourEntity 
    { 
        Id = Guid.NewGuid(), 
        Name = "Sample 1",
        CreatedAt = DateTime.UtcNow 
    },
    new YourEntity 
    { 
        Id = Guid.NewGuid(), 
        Name = "Sample 2",
        CreatedAt = DateTime.UtcNow 
    }
);
```

---

## 6. Best Practices

### 6.1 DTOs (Data Transfer Objects)

**Tại sao cần DTOs?**
- Tách biệt models với API responses
- Security: Không expose sensitive fields
- Performance: Chỉ trả về fields cần thiết
- Versioning: Dễ dàng thay đổi API structure

**Example**:

```csharp
// Models/ViewModels/YourDtos.cs
namespace Hotel_Web.Models.ViewModels
{
    // DTO để trả về client
    public class YourDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Không bao gồm sensitive fields như UserId
    }

    // DTO để nhận khi tạo mới
    public class CreateYourDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    // DTO để update
    public class UpdateYourDto
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
```

### 6.2 Async/Await Pattern

**✅ Đúng**:
```csharp
[HttpGet]
public async Task<ActionResult<List<YourDto>>> GetAll()
{
    var items = await _service.GetAllAsync();
    return Ok(items);
}
```

**❌ Sai**:
```csharp
[HttpGet]
public ActionResult<List<YourDto>> GetAll()
{
    var items = _service.GetAllAsync().Result; // BLOCKING!
    return Ok(items);
}
```

### 6.3 Exception Handling

```csharp
public async Task<ActionResult> YourMethod()
{
    try
    {
        // Your logic
        return Ok(result);
    }
    catch (NotFoundException ex)
    {
        return NotFound(new { message = ex.Message });
    }
    catch (UnauthorizedAccessException ex)
    {
        return Forbid();
    }
    catch (ValidationException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        // Log error
        _logger.LogError(ex, "Error in YourMethod");
        
        // Don't expose details in production
        var message = _env.IsDevelopment() 
            ? ex.Message 
            : "Có lỗi xảy ra";
        
        return StatusCode(500, new { error = message });
    }
}
```

### 6.4 Input Validation

```csharp
// Sử dụng Data Annotations
public class CreateYourDto
{
    [Required(ErrorMessage = "Tên không được để trống")]
    [MaxLength(200, ErrorMessage = "Tên tối đa 200 ký tự")]
    public string Name { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string? Email { get; set; }

    [Range(1, 100, ErrorMessage = "Tuổi phải từ 1 đến 100")]
    public int Age { get; set; }

    [Url(ErrorMessage = "URL không hợp lệ")]
    public string? Website { get; set; }
}

// Trong controller
[HttpPost]
public async Task<ActionResult> Create([FromBody] CreateYourDto dto)
{
    if (!ModelState.IsValid)
    {
        var errors = ModelState
            .Where(x => x.Value.Errors.Any())
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
        
        return BadRequest(new { errors });
    }

    // Continue with logic...
}
```

### 6.5 Authorization

```csharp
// Require authenticated user
[Authorize]
[HttpPost]
public async Task<ActionResult> Create()
{
    // User must be logged in
}

// Require specific role
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<ActionResult> Delete(Guid id)
{
    // Only admins can access
}

// Custom authorization
[HttpPut("{id}")]
[Authorize]
public async Task<ActionResult> Update(Guid id)
{
    var userId = _userManager.GetUserId(User);
    var entity = await _context.YourEntity.FindAsync(id);
    
    if (entity.UserId != userId)
    {
        return Forbid(); // Not the owner
    }
    
    // Continue...
}
```

### 6.6 Response Consistency

```csharp
// Success response wrapper
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; }
}

// Usage
[HttpGet]
public async Task<ActionResult> GetAll()
{
    try
    {
        var data = await _service.GetAllAsync();
        return Ok(new ApiResponse<List<YourDto>>
        {
            Success = true,
            Data = data,
            Message = "Thành công"
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
```

---

## 7. Testing

### 7.1 Unit Test với xUnit

File: `Tests/Services/YourServiceTests.cs`

```csharp
using Xunit;
using Moq;
using Hotel_Web.Services;
using Hotel_Web.Data;
using Microsoft.EntityFrameworkCore;

public class YourServiceTests
{
    private readonly Mock<AppDbContext> _mockContext;
    private readonly YourService _service;

    public YourServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        
        _mockContext = new Mock<AppDbContext>(options);
        _service = new YourService(_mockContext.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsItem_WhenExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        // Setup mock data...

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task CreateAsync_ThrowsException_WhenInvalidData()
    {
        // Arrange
        var invalidDto = new CreateYourDto { Name = "" };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAsync(invalidDto, "user123")
        );
    }
}
```

### 7.2 Integration Test

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class YourAPIIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public YourAPIIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/YourAPI");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }
}
```

### 7.3 Manual Testing với Postman

**Collection Setup**:

1. **Environment Variables**:
```json
{
  "base_url": "https://localhost:7xxx",
  "access_token": "",
  "user_id": ""
}
```

2. **Pre-request Script** (cho Auth):
```javascript
// Set Bearer token
pm.request.headers.add({
    key: 'Authorization',
    value: 'Bearer ' + pm.environment.get('access_token')
});
```

3. **Test Script**:
```javascript
// Verify response
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Response has data", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('data');
});

// Save variables
var jsonData = pm.response.json();
pm.environment.set("created_id", jsonData.data.id);
```

---

## 8. Troubleshooting

### 8.1 Common Issues

#### CORS Error
```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutter", builder =>
    {
        builder.WithOrigins("http://localhost:3000") // Flutter dev URL
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

// ...

app.UseCors("AllowFlutter");
```

#### JSON Serialization Error (Circular Reference)
```csharp
// Program.cs
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = 
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
```

#### EF Core Tracking Issue
```csharp
// Use AsNoTracking for read-only queries
var items = await _context.YourEntity
    .AsNoTracking()
    .ToListAsync();
```

---

## 9. Checklist khi Tạo API Mới

- [ ] Tạo Model trong `Models/`
- [ ] Thêm DbSet vào `AppDbContext`
- [ ] Tạo và chạy Migration
- [ ] Tạo DTO trong `Models/ViewModels/`
- [ ] Tạo Service Interface `IYourService`
- [ ] Implement Service `YourService`
- [ ] Register Service trong `Program.cs`
- [ ] Tạo API Controller `YourAPIController`
- [ ] Test endpoints với Postman
- [ ] Viết Unit Tests
- [ ] Cập nhật `API_DOCUMENTATION.md`
- [ ] Test với Flutter app

---

## 📞 Support

Nếu gặp vấn đề, tham khảo:
- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core Docs](https://docs.microsoft.com/ef/core)
- [RESTful API Best Practices](https://restfulapi.net/)

---

**Happy Coding! 🚀**
