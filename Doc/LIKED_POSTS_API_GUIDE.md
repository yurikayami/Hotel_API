# API Guide - Lấy Bài Viết Yêu Thích (Liked Posts)

## 📌 Tổng Quan

Endpoint này cho phép lấy **danh sách bài viết mà người dùng đã like/yêu thích** với support **infinite scroll**.

---

## 🔐 Authentication

**Bắt buộc đăng nhập** ✅

Tất cả request cần truyền **JWT Bearer Token** trong header:

```
Authorization: Bearer <YOUR_JWT_TOKEN>
```

---

## 📍 Endpoint

```
GET /api/Post/user/likes
```

---

## 📋 Parameters

| Tên | Kiểu | Mặc định | Max | Bắt buộc | Mô tả |
|-----|------|---------|-----|----------|-------|
| `offset` | `int` | `0` | - | ❌ | Số bài viết đã skip (phục vụ pagination) |
| `limit` | `int` | `10` | `50` | ❌ | Số bài viết trả về mỗi request |

### Validation Rules:
- `offset` < 0 → tự động set về 0
- `limit` < 1 → tự động set về 10
- `limit` > 50 → tự động clamp về 50

---

## 📤 Request Example

### cURL:
```bash
curl -X GET \
  "https://localhost:7135/api/Post/user/likes?offset=0&limit=10" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json"
```

### JavaScript/Fetch:
```javascript
const token = 'YOUR_JWT_TOKEN';
const offset = 0;
const limit = 10;

fetch(`https://localhost:7135/api/Post/user/likes?offset=${offset}&limit=${limit}`, {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
})
.then(res => res.json())
.then(data => console.log(data))
.catch(err => console.error(err));
```

### Python/Requests:
```python
import requests

token = "YOUR_JWT_TOKEN"
headers = {
    "Authorization": f"Bearer {token}",
    "Content-Type": "application/json"
}

url = "https://localhost:7135/api/Post/user/likes"
params = {
    "offset": 0,
    "limit": 10
}

response = requests.get(url, headers=headers, params=params)
data = response.json()
print(data)
```

### Dart/Flutter:
```dart
import 'package:http/http.dart' as http;
import 'dart:convert';

Future<Map<String, dynamic>> getUserLikedPosts({
  required String token,
  int offset = 0,
  int limit = 10,
}) async {
  final url = Uri.parse(
    'https://localhost:7135/api/Post/user/likes?offset=$offset&limit=$limit'
  );

  final response = await http.get(
    url,
    headers: {
      'Authorization': 'Bearer $token',
      'Content-Type': 'application/json',
    },
  );

  if (response.statusCode == 200) {
    return jsonDecode(response.body);
  } else if (response.statusCode == 401) {
    throw Exception('Unauthorized - Token không hợp lệ hoặc hết hạn');
  } else {
    throw Exception('Lỗi: ${response.statusCode}');
  }
}
```

---

## 📥 Response Format

### Success Response (Status 200):
```json
{
  "success": true,
  "message": "Danh sách bài viết yêu thích",
  "data": {
    "posts": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "noiDung": "Nội dung bài viết...",
        "loai": "Chia sẻ",
        "duongDanMedia": "https://example.com/uploads/image.jpg",
        "ngayDang": "2025-11-18T10:30:00",
        "luotThich": 25,
        "soBinhLuan": 5,
        "soChiaSe": 3,
        "isLiked": true,
        "hashtags": "#food #healthy #recipe",
        "authorId": "user-123",
        "authorName": "John Doe",
        "authorAvatar": "https://example.com/avatars/user123.jpg"
      },
      {
        "id": "660e8400-e29b-41d4-a716-446655440111",
        "noiDung": "Bài viết thứ hai...",
        "loai": "Bình luận",
        "duongDanMedia": null,
        "ngayDang": "2025-11-17T15:45:00",
        "luotThich": 12,
        "soBinhLuan": 2,
        "soChiaSe": 1,
        "isLiked": true,
        "hashtags": "#news",
        "authorId": "user-456",
        "authorName": "Jane Smith",
        "authorAvatar": "https://example.com/avatars/user456.jpg"
      }
    ],
    "hasMore": true
  },
  "errors": []
}
```

### Error Response (Status 401):
```json
{
  "success": false,
  "message": "Bạn cần đăng nhập để xem bài viết yêu thích",
  "data": null,
  "errors": []
}
```

### Error Response (Status 500):
```json
{
  "success": false,
  "message": "Có lỗi xảy ra khi lấy danh sách bài viết yêu thích",
  "data": null,
  "errors": [
    "Exception message..."
  ]
}
```

---

## 📊 Response Fields

### Main Response Object:
| Field | Kiểu | Mô tả |
|-------|------|-------|
| `success` | `boolean` | Trạng thái request (true = thành công) |
| `message` | `string` | Mô tả kết quả |
| `data` | `object` | Dữ liệu bài viết (xem chi tiết dưới) |
| `errors` | `array` | Danh sách lỗi (nếu có) |

### Data Object:
| Field | Kiểu | Mô tả |
|-------|------|-------|
| `posts` | `array<Post>` | Danh sách bài viết yêu thích |
| `hasMore` | `boolean` | `true` nếu còn bài viết, `false` nếu là trang cuối |

### Post Object:
| Field | Kiểu | Mô tả |
|-------|------|-------|
| `id` | `string` (UUID) | ID bài viết |
| `noiDung` | `string` | Nội dung bài viết |
| `loai` | `string` | Loại bài viết (vd: "Chia sẻ", "Bình luận") |
| `duongDanMedia` | `string` \| `null` | URL ảnh/video (null nếu không có media) |
| `ngayDang` | `datetime` | Ngày đăng bài |
| `luotThich` | `integer` | Số lượt like |
| `soBinhLuan` | `integer` | Số bình luận |
| `soChiaSe` | `integer` | Số lần chia sẻ |
| `isLiked` | `boolean` | Luôn `true` vì đây là bài viết đã like |
| `hashtags` | `string` | Hashtag của bài viết |
| `authorId` | `string` | ID tác giả |
| `authorName` | `string` | Tên tác giả |
| `authorAvatar` | `string` \| `null` | URL avatar tác giả |

---

## 🔄 Infinite Scroll Implementation

### Logic:
1. Gọi API lần đầu với `offset=0, limit=10`
2. Nếu `hasMore=true`, khi user scroll tới cuối, increment `offset += limit`
3. Gọi API lại với `offset=10, limit=10` (và tiếp tục...)
4. Dừng khi `hasMore=false`

### Dart/Flutter Example:
```dart
class LikedPostsScreen extends StatefulWidget {
  @override
  State<LikedPostsScreen> createState() => _LikedPostsScreenState();
}

class _LikedPostsScreenState extends State<LikedPostsScreen> {
  List<Post> likedPosts = [];
  int offset = 0;
  const int limit = 10;
  bool isLoading = false;
  bool hasMore = true;
  final ScrollController _scrollController = ScrollController();
  String token = 'YOUR_JWT_TOKEN'; // Từ login

  @override
  void initState() {
    super.initState();
    _loadLikedPosts();
    _scrollController.addListener(_onScroll);
  }

  void _onScroll() {
    // Kiểm tra nếu scroll tới cuối
    if (_scrollController.position.pixels == 
        _scrollController.position.maxScrollExtent) {
      if (hasMore && !isLoading) {
        _loadLikedPosts();
      }
    }
  }

  Future<void> _loadLikedPosts() async {
    if (isLoading || !hasMore) return;

    setState(() => isLoading = true);

    try {
      final response = await http.get(
        Uri.parse(
          'https://localhost:7135/api/Post/user/likes?offset=$offset&limit=$limit'
        ),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        
        if (data['success']) {
          final posts = (data['data']['posts'] as List)
              .map((p) => Post.fromJson(p))
              .toList();
          
          setState(() {
            likedPosts.addAll(posts);
            offset += limit;
            hasMore = data['data']['hasMore'] ?? false;
            isLoading = false;
          });
        } else {
          throw Exception(data['message']);
        }
      } else if (response.statusCode == 401) {
        // Token hết hạn, redirect to login
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Bạn cần đăng nhập lại')),
        );
      } else {
        throw Exception('Lỗi: ${response.statusCode}');
      }
    } catch (e) {
      setState(() => isLoading = false);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Lỗi: ${e.toString()}')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Bài viết yêu thích')),
      body: ListView.builder(
        controller: _scrollController,
        itemCount: likedPosts.length + (isLoading ? 1 : 0),
        itemBuilder: (context, index) {
          if (index == likedPosts.length) {
            return Padding(
              padding: EdgeInsets.all(16),
              child: CircularProgressIndicator(),
            );
          }
          
          final post = likedPosts[index];
          return PostCard(post: post);
        },
      ),
    );
  }

  @override
  void dispose() {
    _scrollController.dispose();
    super.dispose();
  }
}

class Post {
  final String id;
  final String noiDung;
  final String? loai;
  final String? duongDanMedia;
  final DateTime? ngayDang;
  final int luotThich;
  final int soBinhLuan;
  final int soChiaSe;
  final bool isLiked;
  final String? hashtags;
  final String authorId;
  final String authorName;
  final String? authorAvatar;

  Post({
    required this.id,
    required this.noiDung,
    this.loai,
    this.duongDanMedia,
    this.ngayDang,
    required this.luotThich,
    required this.soBinhLuan,
    required this.soChiaSe,
    required this.isLiked,
    this.hashtags,
    required this.authorId,
    required this.authorName,
    this.authorAvatar,
  });

  factory Post.fromJson(Map<String, dynamic> json) {
    return Post(
      id: json['id'] ?? '',
      noiDung: json['noiDung'] ?? '',
      loai: json['loai'],
      duongDanMedia: json['duongDanMedia'],
      ngayDang: json['ngayDang'] != null 
          ? DateTime.parse(json['ngayDang']) 
          : null,
      luotThich: json['luotThich'] ?? 0,
      soBinhLuan: json['soBinhLuan'] ?? 0,
      soChiaSe: json['soChiaSe'] ?? 0,
      isLiked: json['isLiked'] ?? true,
      hashtags: json['hashtags'],
      authorId: json['authorId'] ?? '',
      authorName: json['authorName'] ?? 'Unknown',
      authorAvatar: json['authorAvatar'],
    );
  }
}

class PostCard extends StatelessWidget {
  final Post post;

  const PostCard({required this.post});

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: EdgeInsets.all(8),
      child: Padding(
        padding: EdgeInsets.all(12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header - Author info
            Row(
              children: [
                CircleAvatar(
                  backgroundImage: post.authorAvatar != null
                      ? NetworkImage(post.authorAvatar!)
                      : null,
                  child: post.authorAvatar == null
                      ? Icon(Icons.person)
                      : null,
                ),
                SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        post.authorName,
                        style: TextStyle(fontWeight: FontWeight.bold),
                      ),
                      Text(
                        post.ngayDang?.toString() ?? '',
                        style: TextStyle(fontSize: 12, color: Colors.grey),
                      ),
                    ],
                  ),
                ),
              ],
            ),
            SizedBox(height: 12),
            // Content
            Text(post.noiDung),
            if (post.hashtags != null && post.hashtags!.isNotEmpty)
              Padding(
                padding: EdgeInsets.only(top: 8),
                child: Text(
                  post.hashtags!,
                  style: TextStyle(color: Colors.blue),
                ),
              ),
            if (post.duongDanMedia != null)
              Padding(
                padding: EdgeInsets.only(top: 12),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(8),
                  child: Image.network(
                    post.duongDanMedia!,
                    fit: BoxFit.cover,
                    errorBuilder: (context, error, stackTrace) {
                      return Container(
                        height: 200,
                        color: Colors.grey[300],
                        child: Icon(Icons.broken_image),
                      );
                    },
                  ),
                ),
              ),
            SizedBox(height: 12),
            // Stats
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceAround,
              children: [
                Row(
                  children: [
                    Icon(Icons.favorite, color: Colors.red),
                    SizedBox(width: 4),
                    Text('${post.luotThich}'),
                  ],
                ),
                Row(
                  children: [
                    Icon(Icons.comment),
                    SizedBox(width: 4),
                    Text('${post.soBinhLuan}'),
                  ],
                ),
                Row(
                  children: [
                    Icon(Icons.share),
                    SizedBox(width: 4),
                    Text('${post.soChiaSe}'),
                  ],
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
```

### JavaScript Example:
```javascript
class LikedPostsManager {
  constructor(token, baseUrl = 'https://localhost:7135') {
    this.token = token;
    this.baseUrl = baseUrl;
    this.likedPosts = [];
    this.offset = 0;
    this.limit = 10;
    this.hasMore = true;
    this.isLoading = false;
  }

  async loadMorePosts() {
    if (this.isLoading || !this.hasMore) return;

    this.isLoading = true;

    try {
      const url = new URL(`${this.baseUrl}/api/Post/user/likes`);
      url.searchParams.append('offset', this.offset);
      url.searchParams.append('limit', this.limit);

      const response = await fetch(url, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${this.token}`,
          'Content-Type': 'application/json'
        }
      });

      if (response.status === 401) {
        throw new Error('Token không hợp lệ, vui lòng đăng nhập lại');
      }

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
      }

      const data = await response.json();

      if (data.success) {
        this.likedPosts.push(...data.data.posts);
        this.offset += this.limit;
        this.hasMore = data.data.hasMore;
        
        return {
          success: true,
          posts: data.data.posts,
          hasMore: this.hasMore
        };
      } else {
        throw new Error(data.message);
      }
    } catch (error) {
      console.error('Lỗi khi tải bài viết yêu thích:', error);
      this.isLoading = false;
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  getAllPosts() {
    return this.likedPosts;
  }

  reset() {
    this.likedPosts = [];
    this.offset = 0;
    this.hasMore = true;
    this.isLoading = false;
  }
}

// Sử dụng:
const manager = new LikedPostsManager('YOUR_JWT_TOKEN');

// Scroll event listener
window.addEventListener('scroll', async () => {
  if ((window.innerHeight + window.scrollY) >= document.body.offsetHeight - 100) {
    try {
      const result = await manager.loadMorePosts();
      console.log(`Đã load ${result.posts.length} bài viết`);
      renderPosts(result.posts);
    } catch (error) {
      console.error('Lỗi:', error.message);
    }
  }
});
```

---

## ⚠️ HTTP Status Codes

| Code | Ý nghĩa | Hành động |
|------|---------|----------|
| `200` | ✅ Thành công | Xử lý dữ liệu bình thường |
| `400` | ❌ Request sai | Kiểm tra parameters (offset, limit) |
| `401` | ❌ Chưa đăng nhập | Redirect user tới login, lấy token mới |
| `403` | ❌ Không có quyền | Hiếm, chỉ nếu token bị revoke |
| `500` | ❌ Lỗi server | Báo lỗi, retry sau vài giây |

---

## 🚨 Error Handling

### Token hết hạn:
```dart
if (response.statusCode == 401) {
  // Xóa token cũ
  await storage.delete(key: 'jwt_token');
  
  // Redirect to login
  Navigator.pushReplacementNamed(context, '/login');
}
```

### Network timeout:
```dart
try {
  await getUserLikedPosts().timeout(Duration(seconds: 10));
} on TimeoutException {
  print('Request timeout - kiểm tra kết nối internet');
}
```

### Retry logic:
```dart
Future<Map> getWithRetry({int maxRetries = 3}) async {
  for (int i = 0; i < maxRetries; i++) {
    try {
      return await getUserLikedPosts(offset: offset, limit: limit);
    } catch (e) {
      if (i == maxRetries - 1) rethrow;
      await Future.delayed(Duration(seconds: 2 * (i + 1)));
    }
  }
}
```

---

## 📝 Common Issues & Solutions

### Issue 1: `hasMore` luôn `true`
**Nguyên nhân**: Bài viết có đúng `limit` số lượng
**Giải pháp**: Load tiếp tục, khi số bài < limit thì `hasMore=false`

### Issue 2: `isLiked` là `false`
**Nguyên nhân**: API trả về sai hoặc cache cũ
**Giải pháp**: Reload page hoặc xóa cache

### Issue 3: Avatar bị null
**Nguyên nhân**: User chưa update avatar
**Giải pháp**: Hiển thị avatar mặc định (icon người)

```dart
CircleAvatar(
  backgroundImage: post.authorAvatar != null 
      ? NetworkImage(post.authorAvatar!)
      : null,
  child: post.authorAvatar == null
      ? Icon(Icons.person)
      : null,
)
```

### Issue 4: 401 Unauthorized khi token hợp lệ
**Nguyên nhân**: Token expire, hoặc format sai
**Giải pháp**: 
- Kiểm tra token format: `Bearer <token>` (có khoảng trắng)
- Đảm bảo token không bị cắt bớt

---

## 💡 Best Practices

1. **Cache locally** - Lưu `likedPosts` vào SQLite/Hive để giảm request
2. **Pagination params** - Khởi tạo `limit=20` để balance giữa tốc độ và dữ liệu
3. **Error UI** - Hiển thị spinner khi loading, toast/snackbar khi error
4. **Refresh token** - Implement auto-refresh token trước khi expire
5. **Offscreen rendering** - Dùng `ListView` lazy loading, không render tất cả
6. **Image optimization** - Resize ảnh trước render, dùng cached_network_image
7. **State management** - Dùng Provider, Riverpod, Bloc để manage state

---

## 🔗 Related Endpoints

- `GET /api/Post/user/posts` - Bài viết của bạn
- `GET /api/Post/user/comments` - Bình luận của bạn
- `POST /api/Post/{id}/like` - Toggle like
- `GET /api/Post/public/{userId}/posts` - Bài viết công khai

---

## 📞 Support

**Issues?** Liên hệ: backend@example.com  
**Last Updated**: 18/11/2025  
**API Version**: 1.0
