# 📋 Hướng Dẫn Test Toàn Diện API Hotel_API

## 📝 Tổng Quan

File test `ApiIntegrationTests.cs` được thiết kế để kiểm tra toàn diện **TẤT CẢ** các endpoint của Hotel API dựa trên cấu trúc database từ file `script.sql`. Bộ test bao gồm 27 test cases chia thành 5 nhóm chính:

### 🎯 Phạm Vi Test

1. **Authentication API (6 tests)** - Register, Login, Logout
2. **Post CRUD API (9 tests)** - Create, Read, Update, Delete posts
3. **Like API (4 tests)** - Like/Unlike posts
4. **Comment API (6 tests)** - Comment và Reply
5. **Integration Flow (2 tests)** - Kịch bản người dùng thực tế

---

## 🚀 Chuẩn Bị Môi Trường Test

### 1. Cài Đặt Dependencies

```bash
cd "d:\Workspace\01 Project\Project Dev\Graduation project\Main Project\Hotel_API"
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 9.0.0
dotnet add package xunit --version 2.9.2
dotnet add package xunit.runner.visualstudio --version 2.8.2
dotnet add package coverlet.collector --version 6.0.2
dotnet add package Moq --version 4.20.72
dotnet add package FluentAssertions --version 6.12.1
dotnet restore
```

### 2. Cấu Hình Database

Đảm bảo `appsettings.json` có connection string đúng:

```json
{
  "ConnectionStrings": {
    "HotelWebConnection": "Data Source=DESKTOP-YURI\\SQLEXPRESS;Initial Catalog=Hotel_Web;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

### 3. Tạo/Cập Nhật Database

```bash
# Tạo migration nếu chưa có
dotnet ef migrations add InitialCreate

# Cập nhật database
dotnet ef database update
```

---

## ▶️ Chạy Tests

### Chạy Tất Cả Tests

```bash
# Chạy tất cả 27 tests
dotnet test

# Chạy với output chi tiết
dotnet test --logger "console;verbosity=detailed"

# Chạy với coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Chạy Theo Category

```bash
# Chỉ test Authentication
dotnet test --filter "Category=Auth"

# Chỉ test Post CRUD
dotnet test --filter "Category=Post"

# Chỉ test Like
dotnet test --filter "Category=Like"

# Chỉ test Comment
dotnet test --filter "Category=Comment"

# Chỉ test Integration Flow
dotnet test --filter "Category=Integration"
```

### Chạy Theo Priority

```bash
# Tests quan trọng nhất
dotnet test --filter "Priority=Critical"

# Tests ưu tiên cao
dotnet test --filter "Priority=High"

# Tests ưu tiên trung bình
dotnet test --filter "Priority=Medium"

# Tests ưu tiên thấp
dotnet test --filter "Priority=Low"
```

### Chạy Test Cụ Thể

```bash
# Test đăng ký thành công
dotnet test --filter "FullyQualifiedName~Test01_Register_Success"

# Test complete user flow
dotnet test --filter "FullyQualifiedName~Test26_CompleteUserFlow"
```

---

## 📊 Chi Tiết Các Test Cases

### 🔐 Group 1: Authentication Tests (Test 01-06)

| Test ID | Test Name | Mục Đích | Priority |
|---------|-----------|----------|----------|
| Test01 | `Register_Success` | Đăng ký user mới thành công | High |
| Test02 | `Register_DuplicateEmail_ReturnsBadRequest` | Email trùng lặp → 400 Bad Request | High |
| Test03 | `Register_InvalidPassword_ReturnsBadRequest` | Password không hợp lệ → 400 | High |
| Test04 | `Login_Success` | Login thành công, nhận JWT token | High |
| Test05 | `Login_InvalidCredentials_ReturnsUnauthorized` | Sai email/password → 401 Unauthorized | High |
| Test06 | `Logout_Success` | Logout thành công, cập nhật trạng thái user | Medium |

**Ví dụ chạy:**
```bash
dotnet test --filter "Category=Auth&Priority=High"
```

### 📝 Group 2: Post CRUD Tests (Test 07-15)

| Test ID | Test Name | Mục Đích | Priority |
|---------|-----------|----------|----------|
| Test07 | `GetPosts_WithoutAuth_ReturnsData` | Lấy danh sách posts không cần auth | High |
| Test08 | `GetPosts_WithPagination_ReturnsCorrectPage` | Phân trang posts đúng | High |
| Test09 | `GetPostById_ValidId_ReturnsPost` | Lấy post theo ID hợp lệ | High |
| Test10 | `GetPostById_InvalidId_ReturnsNotFound` | ID không tồn tại → 404 Not Found | Medium |
| Test11 | `CreatePost_WithAuth_Success` | Tạo post mới thành công | High |
| Test12 | `CreatePost_WithoutAuth_ReturnsUnauthorized` | Tạo post không auth → 401 | High |
| Test13 | `CreatePost_EmptyContent_ReturnsBadRequest` | Nội dung rỗng → 400 | Medium |
| Test14 | `DeletePost_OwnPost_Success` | Xóa post của chính mình | Medium |
| Test15 | `DeletePost_OthersPost_ReturnsForbidden` | Xóa post người khác → 403 Forbidden | Low |

**Ví dụ chạy:**
```bash
dotnet test --filter "Category=Post"
```

### ❤️ Group 3: Like Tests (Test 16-19)

| Test ID | Test Name | Mục Đích | Priority |
|---------|-----------|----------|----------|
| Test16 | `LikePost_FirstTime_AddsLike` | Like lần đầu → thêm like | High |
| Test17 | `LikePost_SecondTime_RemovesLike` | Like lần 2 → bỏ like (toggle) | High |
| Test18 | `LikePost_WithoutAuth_ReturnsUnauthorized` | Like không auth → 401 | Medium |
| Test19 | `LikePost_InvalidPostId_ReturnsNotFound` | Like post không tồn tại → 404 | Low |

**Ví dụ chạy:**
```bash
dotnet test --filter "Category=Like"
```

### 💬 Group 4: Comment Tests (Test 20-25)

| Test ID | Test Name | Mục Đích | Priority |
|---------|-----------|----------|----------|
| Test20 | `GetComments_ValidPostId_ReturnsComments` | Lấy comments của post | High |
| Test21 | `AddComment_Success` | Thêm comment thành công | High |
| Test22 | `AddComment_WithoutAuth_ReturnsUnauthorized` | Comment không auth → 401 | High |
| Test23 | `AddComment_EmptyContent_ReturnsBadRequest` | Nội dung rỗng → 400 | Medium |
| Test24 | `AddReply_Success` | Reply comment (nested comments) | High |
| Test25 | `AddComment_InvalidPostId_ReturnsNotFound` | Comment post không tồn tại → 404 | Low |

**Ví dụ chạy:**
```bash
dotnet test --filter "Category=Comment"
```

### 🔄 Group 5: Integration Flow Tests (Test 26-27)

| Test ID | Test Name | Mục Đích | Priority |
|---------|-----------|----------|----------|
| Test26 | `CompleteUserFlow_RegisterLoginPostLikeComment` | Flow đầy đủ: Đăng ký → Login → Post → Like → Comment → Logout | **Critical** |
| Test27 | `MultipleUsersInteraction_LikeAndCommentSamePost` | Nhiều user tương tác cùng 1 post | High |

**Ví dụ chạy:**
```bash
# Test quan trọng nhất - nên chạy đầu tiên
dotnet test --filter "Priority=Critical"
```

---

## 🎯 Kịch Bản Test Từng Bước

### Kịch Bản 1: Test Basic Flow (5 phút)

```bash
# Bước 1: Test Authentication
dotnet test --filter "Test01_Register_Success"
dotnet test --filter "Test04_Login_Success"

# Bước 2: Test Post CRUD
dotnet test --filter "Test07_GetPosts_WithoutAuth_ReturnsData"
dotnet test --filter "Test11_CreatePost_WithAuth_Success"

# Bước 3: Test Like
dotnet test --filter "Test16_LikePost_FirstTime_AddsLike"

# Bước 4: Test Comment
dotnet test --filter "Test21_AddComment_Success"

# Bước 5: Test Complete Flow
dotnet test --filter "Test26_CompleteUserFlow"
```

### Kịch Bản 2: Test Toàn Bộ (10 phút)

```bash
# Chạy tất cả 27 tests với report chi tiết
dotnet test --logger "console;verbosity=detailed" > test-results.txt

# Xem kết quả
cat test-results.txt
```

### Kịch Bản 3: Test Error Handling

```bash
# Test các trường hợp lỗi
dotnet test --filter "Test02_Register_DuplicateEmail"
dotnet test --filter "Test05_Login_InvalidCredentials"
dotnet test --filter "Test10_GetPostById_InvalidId"
dotnet test --filter "Test12_CreatePost_WithoutAuth"
dotnet test --filter "Test15_DeletePost_OthersPost"
dotnet test --filter "Test18_LikePost_WithoutAuth"
dotnet test --filter "Test22_AddComment_WithoutAuth"
```

---

## 📈 Test Coverage Map

### Endpoints Được Test

| Controller | Endpoint | Method | Tests |
|------------|----------|--------|-------|
| **AuthController** | `/api/Auth/register` | POST | Test01, Test02, Test03 |
| **AuthController** | `/api/Auth/login` | POST | Test04, Test05 |
| **AuthController** | `/api/Auth/logout` | POST | Test06 |
| **PostController** | `/api/Post` | GET | Test07, Test08 |
| **PostController** | `/api/Post/{id}` | GET | Test09, Test10 |
| **PostController** | `/api/Post` | POST | Test11, Test12, Test13 |
| **PostController** | `/api/Post/{id}` | DELETE | Test14, Test15 |
| **PostController** | `/api/Post/{id}/like` | POST | Test16, Test17, Test18, Test19 |
| **PostController** | `/api/Post/{id}/comments` | GET | Test20 |
| **PostController** | `/api/Post/{id}/comments` | POST | Test21, Test22, Test23, Test24, Test25 |

### Database Tables Được Test

✅ **AspNetUsers** - Auth tests  
✅ **AspNetRoles** - Role assignment  
✅ **BaiDang** - Post CRUD tests  
✅ **BaiDang_LuotThich** - Like tests  
✅ **BinhLuan** - Comment tests (bao gồm nested comments với `parent_comment_id`)  

---

## 🔍 Phân Tích Kết Quả Test

### Expected Results (Tất cả tests pass)

```
Passed!  - Failed:     0, Passed:    27, Skipped:     0, Total:    27, Duration: < 1 s
```

### Nếu Test Fail

**Test Authentication Fail:**
- Check connection string
- Check database có tables AspNetUsers, AspNetRoles
- Check JWT configuration trong appsettings.json

**Test Post/Like/Comment Fail:**
- Check tables BaiDang, BaiDang_LuotThich, BinhLuan tồn tại
- Check foreign keys giữa các tables
- Check API server đang chạy

**Test Integration Flow Fail:**
- Chạy từng test riêng lẻ để xác định step nào fail
- Check logs trong console output

---

## 🐛 Troubleshooting

### Lỗi: Connection String

```bash
# Kiểm tra SQL Server đang chạy
services.msc
# Tìm SQL Server (SQLEXPRESS) và start nếu stopped

# Test connection
sqlcmd -S DESKTOP-YURI\SQLEXPRESS -d Hotel_Web -E
```

### Lỗi: Database Not Found

```bash
# Tạo lại database
dotnet ef database drop -f
dotnet ef database update
```

### Lỗi: Test Timeout

```bash
# Tăng timeout
dotnet test -- RunConfiguration.TestSessionTimeout=120000
```

### Lỗi: Port Already In Use

```bash
# Đổi port trong launchSettings.json hoặc kill process
netstat -ano | findstr :5217
taskkill /PID <PID> /F
```

---

## 📊 Test Report Sample

### Success Report
```
✅ Test01_Register_Success - PASS (0.5s)
✅ Test02_Register_DuplicateEmail - PASS (0.3s)
✅ Test03_Register_InvalidPassword - PASS (0.2s)
✅ Test04_Login_Success - PASS (0.4s)
...
✅ Test26_CompleteUserFlow - PASS (2.1s)
✅ Test27_MultipleUsersInteraction - PASS (1.8s)

Total: 27 tests | Passed: 27 | Failed: 0 | Duration: 15.3s
```

---

## 🎓 Best Practices

1. **Chạy tests theo thứ tự Priority:**
   - Critical → High → Medium → Low

2. **Test isolation:**
   - Mỗi test tự tạo data riêng (user, post, comment)
   - Không phụ thuộc vào data có sẵn trong DB

3. **Cleanup:**
   - Tests không tự động xóa data sau khi chạy
   - Nên xóa test data thủ công định kỳ

4. **CI/CD Integration:**
   ```yaml
   # GitHub Actions example
   - name: Run Tests
     run: dotnet test --no-build --verbosity normal
   ```

---

## 📞 Hỗ Trợ

Nếu gặp vấn đề:
1. Check `test-results.txt` để xem log chi tiết
2. Chạy test thất bại riêng lẻ với verbosity=detailed
3. Check API server logs
4. Verify database schema khớp với `script.sql`

---

## 📝 Notes

- Tests sử dụng **xUnit** framework
- API testing với **Microsoft.AspNetCore.Mvc.Testing**
- Test data được tạo tự động với timestamp để tránh conflict
- Mỗi test có thể chạy độc lập (isolated)
- Total execution time: ~15-20 giây cho 27 tests

**Happy Testing! 🎉**
