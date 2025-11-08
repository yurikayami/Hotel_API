using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Hotel_API.Models.ViewModels;

namespace Hotel_API.Tests
{
    /// <summary>
    /// Bộ test tích hợp toàn diện cho Hotel API
    /// Kiểm tra tất cả các endpoints: Auth, Post, Comment, Like
    /// </summary>
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        // Test data
        private const string TestEmail = "testuser@example.com";
        private const string TestPassword = "Test@123";
        private const string TestUsername = "TestUser";

        private string? _authToken;
        private Guid? _testPostId;
        private Guid? _testCommentId;

        public ApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        #region 1. Authentication Tests

        [Fact]
        [Trait("Category", "Auth")]
        [Trait("Priority", "High")]
        public async Task Test01_Register_Success()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = $"user{DateTime.Now.Ticks}@example.com",
                Password = TestPassword,
                ConfirmPassword = TestPassword,
                UserName = $"User{DateTime.Now.Ticks}",
                Gender = "Nam",
                Age = 25
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerDto);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JsonSerializer.Deserialize<AuthResponseDto>(content, _jsonOptions);
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.Equal(registerDto.Email, result?.User?.Email);
            Assert.Equal(registerDto.UserName, result?.User?.UserName);

            // Save token cho các test khác
            _authToken = result?.Token;
        }

        [Fact]
        [Trait("Category", "Auth")]
        [Trait("Priority", "High")]
        public async Task Test02_Register_DuplicateEmail_ReturnsBadRequest()
        {
            // Arrange - Đăng ký lần đầu
            var email = $"duplicate{DateTime.Now.Ticks}@example.com";
            var registerDto = new RegisterDto
            {
                Email = email,
                Password = TestPassword,
                ConfirmPassword = TestPassword,
                UserName = $"DuplicateUser{DateTime.Now.Ticks}",
                Gender = "Nam",
                Age = 25
            };

            await _client.PostAsJsonAsync("/api/Auth/register", registerDto);

            // Act - Đăng ký lần 2 với cùng email
            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "Auth")]
        [Trait("Priority", "High")]
        public async Task Test03_Register_InvalidPassword_ReturnsBadRequest()
        {
            // Arrange - Password quá ngắn
            var registerDto = new RegisterDto
            {
                Email = $"test{DateTime.Now.Ticks}@example.com",
                Password = "123", // Quá ngắn
                ConfirmPassword = "123",
                UserName = $"TestUser{DateTime.Now.Ticks}",
                Gender = "Nam",
                Age = 25
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/register", registerDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "Auth")]
        [Trait("Priority", "High")]
        public async Task Test04_Login_Success()
        {
            // Arrange - Đăng ký user trước
            var email = $"logintest{DateTime.Now.Ticks}@example.com";
            var registerDto = new RegisterDto
            {
                Email = email,
                Password = TestPassword,
                ConfirmPassword = TestPassword,
                UserName = $"LoginUser{DateTime.Now.Ticks}",
                Gender = "Nam",
                Age = 25
            };
            await _client.PostAsJsonAsync("/api/Auth/register", registerDto);

            var loginDto = new LoginDto
            {
                Email = email,
                Password = TestPassword
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JsonSerializer.Deserialize<AuthResponseDto>(content, _jsonOptions);
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.Equal(email, result?.User?.Email);
        }

        [Fact]
        [Trait("Category", "Auth")]
        [Trait("Priority", "High")]
        public async Task Test05_Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "nonexistent@example.com",
                Password = "wrongpassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "Auth")]
        [Trait("Priority", "Medium")]
        public async Task Test06_Logout_Success()
        {
            // Arrange - Đăng ký và login trước
            var token = await RegisterAndLoginAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsync("/api/Auth/logout", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        #region 2. Post CRUD Tests

        [Fact]
        [Trait("Category", "Post")]
        [Trait("Priority", "High")]
        public async Task Test07_GetPosts_WithoutAuth_ReturnsData()
        {
            // Act
            var response = await _client.GetAsync("/api/Post?page=1&pageSize=10");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JsonSerializer.Deserialize<ApiResponse<PostPagedResult>>(content, _jsonOptions);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }

        [Fact]
        [Trait("Category", "Post")]
        [Trait("Priority", "High")]
        public async Task Test08_GetPosts_WithPagination_ReturnsCorrectPage()
        {
            // Act
            var response = await _client.GetAsync("/api/Post?page=1&pageSize=5");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JsonSerializer.Deserialize<ApiResponse<PostPagedResult>>(content, _jsonOptions);
            Assert.NotNull(result?.Data);
            Assert.Equal(1, result.Data.Page);
            Assert.True(result.Data.Posts.Count <= 5);
        }

        [Fact]
        [Trait("Category", "Post")]
        [Trait("Priority", "High")]
        public async Task Test09_GetPostById_ValidId_ReturnsPost()
        {
            // Arrange - Lấy post đầu tiên từ database
            var postsResponse = await _client.GetAsync("/api/Post?page=1&pageSize=1");
            var postsContent = await postsResponse.Content.ReadAsStringAsync();
            var postsResult = JsonSerializer.Deserialize<ApiResponse<PostPagedResult>>(postsContent, _jsonOptions);

            if (postsResult?.Data?.Posts == null || !postsResult.Data.Posts.Any())
            {
                // Skip test nếu không có post nào
                return;
            }

            var postId = postsResult.Data.Posts.First().Id;

            // Act
            var response = await _client.GetAsync($"/api/Post/{postId}");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JsonSerializer.Deserialize<ApiResponse<PostDto>>(content, _jsonOptions);
            Assert.NotNull(result?.Data);
            Assert.Equal(postId, result.Data.Id);
        }

        [Fact]
        [Trait("Category", "Post")]
        [Trait("Priority", "Medium")]
        public async Task Test10_GetPostById_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/Post/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "Post")]
        [Trait("Priority", "High")]
        public async Task Test11_CreatePost_WithAuth_Success()
        {
            // Arrange
            var token = await RegisterAndLoginAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var createPostDto = new CreatePostDto
            {
                NoiDung = "Test post from integration test",
                Hashtags = "#test #integration"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Post", createPostDto);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = JsonSerializer.Deserialize<ApiResponse<PostDto>>(content, _jsonOptions);
            Assert.NotNull(result?.Data);
            Assert.Equal(createPostDto.NoiDung, result.Data.NoiDung);

            _testPostId = result.Data.Id;
        }

        [Fact]
        [Trait("Category", "Post")]
        [Trait("Priority", "High")]
        public async Task Test12_CreatePost_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            var createPostDto = new CreatePostDto
            {
                NoiDung = "Test post without auth"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Post", createPostDto);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "Post")]
        [Trait("Priority", "Medium")]
        public async Task Test13_CreatePost_EmptyContent_ReturnsBadRequest()
        {
            // Arrange
            var token = await RegisterAndLoginAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var createPostDto = new CreatePostDto
            {
                NoiDung = "" // Nội dung rỗng
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/Post", createPostDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "Post")]
        [Trait("Priority", "Medium")]
        public async Task Test14_DeletePost_OwnPost_Success()
        {
            // Arrange - Tạo post trước
            var token = await RegisterAndLoginAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var createPostDto = new CreatePostDto
            {
                NoiDung = "Post to be deleted"
            };
            var createResponse = await _client.PostAsJsonAsync("/api/Post", createPostDto);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<ApiResponse<PostDto>>(createContent, _jsonOptions);
            var postId = createResult!.Data!.Id;

            // Act
            var response = await _client.DeleteAsync($"/api/Post/{postId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "Post")]
        [Trait("Priority", "Low")]
        public async Task Test15_DeletePost_OthersPost_ReturnsForbidden()
        {
            // Arrange - User 1 tạo post
            var token1 = await RegisterAndLoginAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

            var createPostDto = new CreatePostDto
            {
                NoiDung = "User 1 post"
            };
            var createResponse = await _client.PostAsJsonAsync("/api/Post", createPostDto);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<ApiResponse<PostDto>>(createContent, _jsonOptions);
            var postId = createResult!.Data!.Id;

            // User 2 cố xóa post của user 1
            var token2 = await RegisterAndLoginAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

            // Act
            var response = await _client.DeleteAsync($"/api/Post/{postId}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        #endregion

        #region 3. Like Tests

        [Fact]
        [Trait("Category", "Like")]
        [Trait("Priority", "High")]
        public async Task Test16_LikePost_FirstTime_AddsLike()
        {
            // Arrange - Tạo post và user
            var token = await RegisterAndLoginAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var createPostDto = new CreatePostDto
            {
                NoiDung = "Post to like"
            };
            var createResponse = await _client.PostAsJsonAsync("/api/Post", createPostDto);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<ApiResponse<PostDto>>(createContent, _jsonOptions);
            var postId = createResult!.Data!.Id;

            // Act
            var response = await _client.PostAsync($"/api/Post/{postId}/like", null);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JsonSerializer.Deserialize<ApiResponse<object>>(content, _jsonOptions);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Contains("thích", result.Message.ToLower());
        }

        [Fact]
        [Trait("Category", "Like")]
        [Trait("Priority", "High")]
        public async Task Test17_LikePost_SecondTime_RemovesLike()
        {
            // Arrange - Tạo post và like lần đầu
            var token = await RegisterAndLoginAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var createPostDto = new CreatePostDto
            {
                NoiDung = "Post to like and unlike"
            };
            var createResponse = await _client.PostAsJsonAsync("/api/Post", createPostDto);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<ApiResponse<PostDto>>(createContent, _jsonOptions);
            var postId = createResult!.Data!.Id;

            // Like lần đầu
            await _client.PostAsync($"/api/Post/{postId}/like", null);

            // Act - Like lần 2 (unlike)
            var response = await _client.PostAsync($"/api/Post/{postId}/like", null);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JsonSerializer.Deserialize<ApiResponse<object>>(content, _jsonOptions);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Contains("bỏ thích", result.Message.ToLower());
        }

        [Fact]
        [Trait("Category", "Like")]
        [Trait("Priority", "Medium")]
        public async Task Test18_LikePost_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            var postId = Guid.NewGuid();

            // Act
            var response = await _client.PostAsync($"/api/Post/{postId}/like", null);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "Like")]
        [Trait("Priority", "Low")]
        public async Task Test19_LikePost_InvalidPostId_ReturnsNotFound()
        {
            // Arrange
            var token = await RegisterAndLoginAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var invalidPostId = Guid.NewGuid();

            // Act
            var response = await _client.PostAsync($"/api/Post/{invalidPostId}/like", null);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region 4. Comment Tests

        [Fact]
        [Trait("Category", "Comment")]
        [Trait("Priority", "High")]
        public async Task Test20_GetComments_ValidPostId_ReturnsComments()
        {
            // Arrange - Lấy post đầu tiên
            var postsResponse = await _client.GetAsync("/api/Post?page=1&pageSize=1");
            var postsContent = await postsResponse.Content.ReadAsStringAsync();
            var postsResult = JsonSerializer.Deserialize<ApiResponse<PostPagedResult>>(postsContent, _jsonOptions);

            if (postsResult?.Data?.Posts == null || !postsResult.Data.Posts.Any())
            {
                return;
            }

            var postId = postsResult.Data.Posts.First().Id;

            // Act
            var response = await _client.GetAsync($"/api/Post/{postId}/comments");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JsonSerializer.Deserialize<ApiResponse<List<CommentDto>>>(content, _jsonOptions);
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        [Trait("Category", "Comment")]
        [Trait("Priority", "High")]
        public async Task Test21_AddComment_Success()
        {
            // Arrange - Tạo post trước
            var token = await RegisterAndLoginAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var createPostDto = new CreatePostDto
            {
                NoiDung = "Post to comment on"
            };
            var createResponse = await _client.PostAsJsonAsync("/api/Post", createPostDto);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<ApiResponse<PostDto>>(createContent, _jsonOptions);
            var postId = createResult!.Data!.Id;

            var commentDto = new CreateCommentDto
            {
                NoiDung = "Test comment"
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/Post/{postId}/comments", commentDto);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = JsonSerializer.Deserialize<ApiResponse<CommentDto>>(content, _jsonOptions);
            Assert.NotNull(result?.Data);
            Assert.Equal(commentDto.NoiDung, result.Data.NoiDung);

            _testCommentId = result.Data.Id;
        }

        [Fact]
        [Trait("Category", "Comment")]
        [Trait("Priority", "High")]
        public async Task Test22_AddComment_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            var postId = Guid.NewGuid();
            var commentDto = new CreateCommentDto
            {
                NoiDung = "Test comment without auth"
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/Post/{postId}/comments", commentDto);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "Comment")]
        [Trait("Priority", "Medium")]
        public async Task Test23_AddComment_EmptyContent_ReturnsBadRequest()
        {
            // Arrange
            var token = await RegisterAndLoginAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var createPostDto = new CreatePostDto
            {
                NoiDung = "Post to comment on"
            };
            var createResponse = await _client.PostAsJsonAsync("/api/Post", createPostDto);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<ApiResponse<PostDto>>(createContent, _jsonOptions);
            var postId = createResult!.Data!.Id;

            var commentDto = new CreateCommentDto
            {
                NoiDung = "" // Nội dung rỗng
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/Post/{postId}/comments", commentDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "Comment")]
        [Trait("Priority", "High")]
        public async Task Test24_AddReply_Success()
        {
            // Arrange - Tạo post và comment cha trước
            var token = await RegisterAndLoginAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Tạo post
            var createPostDto = new CreatePostDto
            {
                NoiDung = "Post for reply test"
            };
            var createResponse = await _client.PostAsJsonAsync("/api/Post", createPostDto);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<ApiResponse<PostDto>>(createContent, _jsonOptions);
            var postId = createResult!.Data!.Id;

            // Tạo comment cha
            var parentCommentDto = new CreateCommentDto
            {
                NoiDung = "Parent comment"
            };
            var parentResponse = await _client.PostAsJsonAsync($"/api/Post/{postId}/comments", parentCommentDto);
            var parentContent = await parentResponse.Content.ReadAsStringAsync();
            var parentResult = JsonSerializer.Deserialize<ApiResponse<CommentDto>>(parentContent, _jsonOptions);
            var parentCommentId = parentResult!.Data!.Id;

            // Tạo reply
            var replyDto = new CreateCommentDto
            {
                NoiDung = "Reply to parent",
                ParentCommentId = parentCommentId
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/Post/{postId}/comments", replyDto);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = JsonSerializer.Deserialize<ApiResponse<CommentDto>>(content, _jsonOptions);
            Assert.NotNull(result?.Data);
            Assert.Equal(replyDto.NoiDung, result.Data.NoiDung);
            Assert.Equal(parentCommentId, result.Data.ParentCommentId);
        }

        [Fact]
        [Trait("Category", "Comment")]
        [Trait("Priority", "Low")]
        public async Task Test25_AddComment_InvalidPostId_ReturnsNotFound()
        {
            // Arrange
            var token = await RegisterAndLoginAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var invalidPostId = Guid.NewGuid();
            var commentDto = new CreateCommentDto
            {
                NoiDung = "Comment on invalid post"
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/Post/{invalidPostId}/comments", commentDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region 5. Integration Flow Tests

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Priority", "Critical")]
        public async Task Test26_CompleteUserFlow_RegisterLoginPostLikeComment()
        {
            // 1. Register
            var email = $"flowtest{DateTime.Now.Ticks}@example.com";
            var registerDto = new RegisterDto
            {
                Email = email,
                Password = TestPassword,
                ConfirmPassword = TestPassword,
                UserName = $"FlowUser{DateTime.Now.Ticks}",
                Gender = "Nam",
                Age = 25
            };
            var registerResponse = await _client.PostAsJsonAsync("/api/Auth/register", registerDto);
            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

            // 2. Login
            var loginDto = new LoginDto
            {
                Email = email,
                Password = TestPassword
            };
            var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<AuthResponseDto>(loginContent, _jsonOptions);

            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
            Assert.NotNull(loginResult?.Token);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

            // 3. Create Post
            var createPostDto = new CreatePostDto
            {
                NoiDung = "Complete flow test post",
                Hashtags = "#flowtest"
            };
            var postResponse = await _client.PostAsJsonAsync("/api/Post", createPostDto);
            var postContent = await postResponse.Content.ReadAsStringAsync();
            var postResult = JsonSerializer.Deserialize<ApiResponse<PostDto>>(postContent, _jsonOptions);

            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
            Assert.NotNull(postResult?.Data);
            var postId = postResult.Data.Id;

            // 4. Like Post
            var likeResponse = await _client.PostAsync($"/api/Post/{postId}/like", null);
            Assert.Equal(HttpStatusCode.OK, likeResponse.StatusCode);

            // 5. Comment on Post
            var commentDto = new CreateCommentDto
            {
                NoiDung = "Flow test comment"
            };
            var commentResponse = await _client.PostAsJsonAsync($"/api/Post/{postId}/comments", commentDto);
            Assert.Equal(HttpStatusCode.Created, commentResponse.StatusCode);

            // 6. Get Post with updated like and comment count
            var getPostResponse = await _client.GetAsync($"/api/Post/{postId}");
            var getPostContent = await getPostResponse.Content.ReadAsStringAsync();
            var getPostResult = JsonSerializer.Deserialize<ApiResponse<PostDto>>(getPostContent, _jsonOptions);

            Assert.Equal(HttpStatusCode.OK, getPostResponse.StatusCode);
            Assert.NotNull(getPostResult?.Data);
            Assert.True(getPostResult.Data.LuotThich > 0);
            Assert.True(getPostResult.Data.SoBinhLuan > 0);

            // 7. Logout
            var logoutResponse = await _client.PostAsync("/api/Auth/logout", null);
            Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Priority", "High")]
        public async Task Test27_MultipleUsersInteraction_LikeAndCommentSamePost()
        {
            // User 1 tạo post
            var token1 = await RegisterAndLoginAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

            var createPostDto = new CreatePostDto
            {
                NoiDung = "Post for multiple users interaction"
            };
            var postResponse = await _client.PostAsJsonAsync("/api/Post", createPostDto);
            var postContent = await postResponse.Content.ReadAsStringAsync();
            var postResult = JsonSerializer.Deserialize<ApiResponse<PostDto>>(postContent, _jsonOptions);
            var postId = postResult!.Data!.Id;

            // User 2 like và comment
            var token2 = await RegisterAndLoginAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

            var likeResponse = await _client.PostAsync($"/api/Post/{postId}/like", null);
            Assert.Equal(HttpStatusCode.OK, likeResponse.StatusCode);

            var commentDto = new CreateCommentDto
            {
                NoiDung = "User 2 comment"
            };
            var commentResponse = await _client.PostAsJsonAsync($"/api/Post/{postId}/comments", commentDto);
            Assert.Equal(HttpStatusCode.Created, commentResponse.StatusCode);

            // User 3 like và comment
            var token3 = await RegisterAndLoginAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token3);

            likeResponse = await _client.PostAsync($"/api/Post/{postId}/like", null);
            Assert.Equal(HttpStatusCode.OK, likeResponse.StatusCode);

            commentDto = new CreateCommentDto
            {
                NoiDung = "User 3 comment"
            };
            commentResponse = await _client.PostAsJsonAsync($"/api/Post/{postId}/comments", commentDto);
            Assert.Equal(HttpStatusCode.Created, commentResponse.StatusCode);

            // Verify post has 2 likes and 2 comments
            var getPostResponse = await _client.GetAsync($"/api/Post/{postId}");
            var getPostContent = await getPostResponse.Content.ReadAsStringAsync();
            var getPostResult = JsonSerializer.Deserialize<ApiResponse<PostDto>>(getPostContent, _jsonOptions);

            Assert.NotNull(getPostResult?.Data);
            Assert.Equal(2, getPostResult.Data.LuotThich);
            Assert.Equal(2, getPostResult.Data.SoBinhLuan);
        }

        #endregion

        #region Helper Methods

        private async Task<string> RegisterAndLoginAsync()
        {
            var email = $"user{DateTime.Now.Ticks}@example.com";
            var registerDto = new RegisterDto
            {
                Email = email,
                Password = TestPassword,
                ConfirmPassword = TestPassword,
                UserName = $"User{DateTime.Now.Ticks}",
                Gender = "Nam",
                Age = 25
            };

            await _client.PostAsJsonAsync("/api/Auth/register", registerDto);

            var loginDto = new LoginDto
            {
                Email = email,
                Password = TestPassword
            };

            var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AuthResponseDto>(content, _jsonOptions);

            return result!.Token!;
        }

        #endregion
    }
}
