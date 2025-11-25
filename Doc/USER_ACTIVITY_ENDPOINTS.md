# User Activity Endpoints Documentation

## Overview
Three new API endpoints have been implemented to retrieve user-specific data: liked posts, created posts, and comments.

---

## 1. Get User's Liked Posts

### Endpoint
```
GET /api/Post/user/likes?page=1&pageSize=10
```

### Authentication
- **Required**: Yes (`[Authorize]`)
- **Method**: Bearer Token (JWT)

### Query Parameters
| Parameter | Type | Default | Min | Max | Description |
|-----------|------|---------|-----|-----|-------------|
| `page` | int | 1 | 1 | ∞ | Current page number |
| `pageSize` | int | 10 | 1 | 50 | Number of posts per page |

### Response (200 OK)
```json
{
  "success": true,
  "message": "Danh sách bài viết yêu thích",
  "data": {
    "posts": [
      {
        "id": "00000000-0000-0000-0000-000000000001",
        "noiDung": "Post content",
        "loai": "Chia sẻ",
        "duongDanMedia": "https://example.com/uploads/image.jpg",
        "ngayDang": "2025-11-17T10:30:00",
        "luotThich": 15,
        "soBinhLuan": 3,
        "soChiaSe": 2,
        "isLiked": true,
        "hashtags": "#food #recipe",
        "authorId": "user-id-123",
        "authorName": "John Doe",
        "authorAvatar": "https://example.com/avatars/user.jpg"
      }
    ],
    "totalCount": 25,
    "page": 1,
    "pageSize": 10,
    "totalPages": 3,
    "hasPrevious": false,
    "hasNext": true
  },
  "errors": []
}
```

### Error Responses
- **401 Unauthorized**: User not authenticated
- **500 Internal Server Error**: Database or server error

### Example Request
```bash
curl -X GET "http://localhost:7043/api/Post/user/likes?page=1&pageSize=10" \
  -H "Authorization: Bearer {token}"
```

---

## 2. Get User's Created Posts

### Endpoint
```
GET /api/Post/user/posts?page=1&pageSize=10
```

### Authentication
- **Required**: Yes (`[Authorize]`)
- **Method**: Bearer Token (JWT)

### Query Parameters
| Parameter | Type | Default | Min | Max | Description |
|-----------|------|---------|-----|-----|-------------|
| `page` | int | 1 | 1 | ∞ | Current page number |
| `pageSize` | int | 10 | 1 | 50 | Number of posts per page |

### Response (200 OK)
```json
{
  "success": true,
  "message": "Danh sách bài viết của bạn",
  "data": {
    "posts": [
      {
        "id": "00000000-0000-0000-0000-000000000001",
        "noiDung": "My post content",
        "loai": "Chia sẻ",
        "duongDanMedia": "https://example.com/uploads/image.jpg",
        "ngayDang": "2025-11-17T10:30:00",
        "luotThich": 10,
        "soBinhLuan": 5,
        "soChiaSe": 1,
        "isLiked": false,
        "hashtags": "#cooking #healthy",
        "authorId": "user-id-123",
        "authorName": "John Doe",
        "authorAvatar": "https://example.com/avatars/user.jpg"
      }
    ],
    "totalCount": 42,
    "page": 1,
    "pageSize": 10,
    "totalPages": 5,
    "hasPrevious": false,
    "hasNext": true
  },
  "errors": []
}
```

### Features
- Returns posts created by the current user
- Includes `isLiked` status for each post (whether current user liked it)
- Sorted by creation date (newest first)
- Shows empty list if user has no posts

### Example Request
```bash
curl -X GET "http://localhost:7043/api/Post/user/posts?page=1&pageSize=20" \
  -H "Authorization: Bearer {token}"
```

---

## 3. Get User's Created Comments

### Endpoint
```
GET /api/Post/user/comments?page=1&pageSize=10
```

### Authentication
- **Required**: Yes (`[Authorize]`)
- **Method**: Bearer Token (JWT)

### Query Parameters
| Parameter | Type | Default | Min | Max | Description |
|-----------|------|---------|-----|-----|-------------|
| `page` | int | 1 | 1 | ∞ | Current page number |
| `pageSize` | int | 10 | 1 | 50 | Number of comments per page |

### Response (200 OK)
```json
{
  "success": true,
  "message": "Danh sách bình luận của bạn",
  "data": {
    "comments": [
      {
        "id": "00000000-0000-0000-0000-000000000001",
        "noiDung": "Great recipe! Will try it.",
        "ngayTao": "2025-11-17T09:15:00",
        "parentCommentId": null,
        "userId": "user-id-123",
        "userName": "John Doe",
        "userAvatar": "https://example.com/avatars/user.jpg",
        "replies": []
      }
    ],
    "totalCount": 18,
    "page": 1,
    "pageSize": 10,
    "totalPages": 2,
    "hasPrevious": false,
    "hasNext": true
  },
  "errors": []
}
```

### Features
- Returns only **parent comments** (non-reply comments) for pagination efficiency
- Sorted by creation date (newest first)
- `replies` array is empty (replies can be loaded separately if needed)
- Shows empty list if user has no comments

### Example Request
```bash
curl -X GET "http://localhost:7043/api/Post/user/comments?page=1&pageSize=15" \
  -H "Authorization: Bearer {token}"
```

---

## Database Queries (SQL Reference)

### Liked Posts Query
```sql
SELECT * FROM BaiDang_LuotThich
WHERE nguoidung_id = '728b7060-5a5c-4e25-a034-24cfde225029'
ORDER BY ngay_thich DESC
```

### User Posts Query
```sql
SELECT * FROM BaiDang
WHERE NguoiDungId = '40d05072-2ecc-40cb-8198-2caac5b78da5'
ORDER BY NgayDang DESC
```

### User Comments Query
```sql
SELECT * FROM BinhLuan
WHERE id_nguoi_dung = '40d05072-2ecc-40cb-8198-1caac5b78da5'
  AND parent_comment_id IS NULL
ORDER BY ngay_binh_luan DESC
```

---

## Response DTOs

### PostPagedResult
```csharp
public class PostPagedResult
{
    public List<PostDto> Posts { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }      // Calculated
    public bool HasPrevious { get; set; }    // Calculated
    public bool HasNext { get; set; }        // Calculated
}
```

### CommentPagedResult
```csharp
public class CommentPagedResult
{
    public List<CommentDto> Comments { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }      // Calculated
    public bool HasPrevious { get; set; }    // Calculated
    public bool HasNext { get; set; }        // Calculated
}
```

---

## Error Handling
    
All endpoints follow standard error response format:

```json
{
  "success": false,
  "message": "Bạn cần đăng nhập để xem bài viết yêu thích",
  "data": null,
  "errors": []
}
```

### Common Error Codes
| Status | Message | Description |
|--------|---------|-------------|
| 401 | Bạn cần đăng nhập | User not authenticated |
| 500 | Có lỗi xảy ra | Server or database error |

---

## Implementation Notes

1. **Authentication**: All endpoints require valid JWT token in Authorization header
2. **Pagination**: Invalid page/pageSize values are automatically corrected (min 1, max 50 for pageSize)
3. **Media URLs**: All image URLs are converted from relative paths to full URLs using `MediaUrlService`
4. **Performance**: 
   - Liked posts are sorted by like date
   - User posts are sorted by creation date (newest first)
   - Comments are returned as parent comments only for efficiency
5. **Authorization**: Each user can only view their own data

---

## Testing

### Using REST Client (VS Code Extension)

```http
### Get User's Liked Posts
GET http://localhost:7043/api/Post/user/likes?page=1&pageSize=10
Authorization: Bearer {your-jwt-token}

### Get User's Posts
GET http://localhost:7043/api/Post/user/posts?page=1&pageSize=10
Authorization: Bearer {your-jwt-token}

### Get User's Comments
GET http://localhost:7043/api/Post/user/comments?page=1&pageSize=10
Authorization: Bearer {your-jwt-token}
```

---

## Related Endpoints

- `POST /api/Post/{id}/like` - Like/Unlike a post
- `GET /api/Post/{id}/comments` - Get all comments for a post
- `POST /api/Post/{id}/comments` - Add a comment to a post
- `GET /api/Post` - Get home feed with mixed content
- `GET /api/Post/{id}` - Get post details
