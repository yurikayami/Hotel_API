# Flutter API Guide - User Activity & Posts

## 📱 Giới thiệu

Tài liệu này hướng dẫn Flutter developers cách sử dụng các API liên quan đến:
- 📝 **Posts** (Bài viết)
- 💬 **Comments** (Bình luận)
- ❤️ **Likes** (Yêu thích)
- 👤 **User Activity** (Hoạt động người dùng)

---

## 🔐 Authentication

Tất cả API (ngoại trừ public endpoints) yêu cầu **JWT Bearer Token**.

### Cách gửi token:
```dart
final headers = {
  'Authorization': 'Bearer YOUR_JWT_TOKEN_HERE',
  'Content-Type': 'application/json',
};
```

### Lấy token (Login):
```dart
Future<String?> login(String email, String password) async {
  final response = await http.post(
    Uri.parse('$BASE_URL/api/Auth/login'),
    headers: {'Content-Type': 'application/json'},
    body: jsonEncode({
      'email': email,
      'password': password,
    }),
  );

  if (response.statusCode == 200) {
    final data = jsonDecode(response.body);
    return data['data']['token']; // Lưu token này
  }
  return null;
}
```

---

## 📝 Posts API

### 1. Lấy danh sách bài viết của bạn (Infinity Scroll)

**Endpoint:** `GET /api/Post/user/posts`  
**Auth:** ✅ Yêu cầu đăng nhập  
**Parameters:**
- `offset` (int, default: 0) - Số bài viết đã skip
- `limit` (int, default: 10, max: 50) - Số bài viết trả về

**Response:**
```json
{
  "success": true,
  "message": "Danh sách bài viết của bạn",
  "data": {
    "posts": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "noiDung": "Nội dung bài viết...",
        "loai": "Chia sẻ",
        "duongDanMedia": "https://example.com/uploads/image.jpg",
        "ngayDang": "2025-11-17T10:30:00",
        "luotThich": 15,
        "soBinhLuan": 3,
        "soChiaSe": 2,
        "isLiked": false,
        "hashtags": "#food #recipe",
        "authorId": "user-id-123",
        "authorName": "John Doe",
        "authorAvatar": "https://example.com/avatars/user.jpg"
      }
    ],
    "hasMore": true
  },
  "errors": []
}
```

**Dart Code:**
```dart
import 'package:http/http.dart' as http;
import 'dart:convert';

class PostService {
  static const String baseUrl = 'https://localhost:7135';
  static const String token = 'YOUR_JWT_TOKEN';

  static Future<List<Post>> getUserPosts({required int offset, int limit = 10}) async {
    final url = Uri.parse('$baseUrl/api/Post/user/posts?offset=$offset&limit=$limit');
    
    final response = await http.get(
      url,
      headers: {
        'Authorization': 'Bearer $token',
        'Content-Type': 'application/json',
      },
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      final posts = (data['data']['posts'] as List)
          .map((p) => Post.fromJson(p))
          .toList();
      final hasMore = data['data']['hasMore'] ?? false;
      
      return posts;
    } else if (response.statusCode == 401) {
      throw Exception('Bạn cần đăng nhập');
    }
    throw Exception('Lỗi tải bài viết');
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
      ngayDang: json['ngayDang'] != null ? DateTime.parse(json['ngayDang']) : null,
      luotThich: json['luotThich'] ?? 0,
      soBinhLuan: json['soBinhLuan'] ?? 0,
      soChiaSe: json['soChiaSe'] ?? 0,
      isLiked: json['isLiked'] ?? false,
      hashtags: json['hashtags'],
      authorId: json['authorId'] ?? '',
      authorName: json['authorName'] ?? 'Unknown',
      authorAvatar: json['authorAvatar'],
    );
  }
}
```

**Flutter UI - Infinity Scroll:**
```dart
import 'package:flutter/material.dart';

class UserPostsScreen extends StatefulWidget {
  @override
  State<UserPostsScreen> createState() => _UserPostsScreenState();
}

class _UserPostsScreenState extends State<UserPostsScreen> {
  final List<Post> posts = [];
  int offset = 0;
  bool isLoading = false;
  bool hasMore = true;
  final ScrollController _scrollController = ScrollController();

  @override
  void initState() {
    super.initState();
    _loadMorePosts();
    _scrollController.addListener(_onScroll);
  }

  void _onScroll() {
    if (_scrollController.position.pixels == 
        _scrollController.position.maxScrollExtent) {
      if (hasMore && !isLoading) {
        _loadMorePosts();
      }
    }
  }

  Future<void> _loadMorePosts() async {
    if (isLoading) return;
    
    setState(() => isLoading = true);
    
    try {
      final newPosts = await PostService.getUserPosts(offset: offset);
      
      setState(() {
        posts.addAll(newPosts);
        offset += 10;
        hasMore = newPosts.length == 10;
        isLoading = false;
      });
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
      appBar: AppBar(title: Text('Bài viết của tôi')),
      body: ListView.builder(
        controller: _scrollController,
        itemCount: posts.length + (isLoading ? 1 : 0),
        itemBuilder: (context, index) {
          if (index == posts.length) {
            return Center(child: CircularProgressIndicator());
          }
          
          final post = posts[index];
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
            // Header - Author
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
                Column(
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
              ],
            ),
            SizedBox(height: 12),
            // Content
            Text(post.noiDung),
            SizedBox(height: 12),
            // Image
            if (post.duongDanMedia != null)
              ClipRRect(
                borderRadius: BorderRadius.circular(8),
                child: Image.network(
                  post.duongDanMedia!,
                  fit: BoxFit.cover,
                ),
              ),
            SizedBox(height: 12),
            // Stats - Like, Comment, Share
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceAround,
              children: [
                Row(
                  children: [
                    Icon(Icons.favorite,
                        color: post.isLiked ? Colors.red : Colors.grey),
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

---

### 2. Lấy bài viết của người dùng khác (Public)

**Endpoint:** `GET /api/Post/public/{userId}/posts`  
**Auth:** ❌ Không yêu cầu  
**Parameters:**
- `userId` (string) - ID của người dùng
- `offset` (int, default: 0)
- `limit` (int, default: 10, max: 50)

**Dart Code:**
```dart
Future<List<Post>> getPublicUserPosts({
  required String userId,
  required int offset,
  int limit = 10,
}) async {
  final url = Uri.parse(
    '$baseUrl/api/Post/public/$userId/posts?offset=$offset&limit=$limit'
  );
  
  final response = await http.get(url);
  
  if (response.statusCode == 200) {
    final data = jsonDecode(response.body);
    return (data['data']['posts'] as List)
        .map((p) => Post.fromJson(p))
        .toList();
  }
  throw Exception('Lỗi tải bài viết');
}
```

---

### 3. Like/Unlike bài viết

**Endpoint:** `POST /api/Post/{id}/like`  
**Auth:** ✅ Yêu cầu đăng nhập  
**Request Body:** (Không cần)

**Response:**
```json
{
  "success": true,
  "message": "Thích bài viết thành công",
  "data": {
    "isLiked": true,
    "likeCount": 16
  }
}
```

**Dart Code:**
```dart
Future<Map<String, dynamic>> toggleLike(String postId) async {
  final url = Uri.parse('$baseUrl/api/Post/$postId/like');
  
  final response = await http.post(
    url,
    headers: {
      'Authorization': 'Bearer $token',
      'Content-Type': 'application/json',
    },
  );

  if (response.statusCode == 200) {
    final data = jsonDecode(response.body);
    return {
      'isLiked': data['data']['isLiked'],
      'likeCount': data['data']['likeCount'],
    };
  }
  throw Exception('Lỗi like bài viết');
}
```

**Flutter - Like Button:**
```dart
class LikeButton extends StatefulWidget {
  final String postId;
  final bool initialLiked;
  final int initialCount;

  const LikeButton({
    required this.postId,
    required this.initialLiked,
    required this.initialCount,
  });

  @override
  State<LikeButton> createState() => _LikeButtonState();
}

class _LikeButtonState extends State<LikeButton> {
  late bool isLiked;
  late int likeCount;
  bool isLoading = false;

  @override
  void initState() {
    super.initState();
    isLiked = widget.initialLiked;
    likeCount = widget.initialCount;
  }

  Future<void> _toggleLike() async {
    setState(() => isLoading = true);
    
    try {
      final result = await PostService.toggleLike(widget.postId);
      setState(() {
        isLiked = result['isLiked'];
        likeCount = result['likeCount'];
        isLoading = false;
      });
    } catch (e) {
      setState(() => isLoading = false);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Lỗi: ${e.toString()}')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: isLoading ? null : _toggleLike,
      child: Row(
        children: [
          Icon(
            isLiked ? Icons.favorite : Icons.favorite_border,
            color: isLiked ? Colors.red : Colors.grey,
          ),
          SizedBox(width: 4),
          Text('$likeCount'),
        ],
      ),
    );
  }
}
```

---

## 💬 Comments API

### 1. Lấy bình luận của bạn

**Endpoint:** `GET /api/Post/user/comments`  
**Auth:** ✅ Yêu cầu đăng nhập  
**Parameters:**
- `offset` (int, default: 0)
- `limit` (int, default: 10, max: 50)

**Dart Code:**
```dart
Future<List<Comment>> getUserComments({required int offset, int limit = 10}) async {
  final url = Uri.parse('$baseUrl/api/Post/user/comments?offset=$offset&limit=$limit');
  
  final response = await http.get(
    url,
    headers: {'Authorization': 'Bearer $token'},
  );

  if (response.statusCode == 200) {
    final data = jsonDecode(response.body);
    return (data['data']['comments'] as List)
        .map((c) => Comment.fromJson(c))
        .toList();
  }
  throw Exception('Lỗi tải bình luận');
}

class Comment {
  final String id;
  final String noiDung;
  final DateTime ngayTao;
  final String? parentCommentId;
  final String userId;
  final String userName;
  final String? userAvatar;

  Comment({
    required this.id,
    required this.noiDung,
    required this.ngayTao,
    this.parentCommentId,
    required this.userId,
    required this.userName,
    this.userAvatar,
  });

  factory Comment.fromJson(Map<String, dynamic> json) {
    return Comment(
      id: json['id'] ?? '',
      noiDung: json['noiDung'] ?? '',
      ngayTao: DateTime.parse(json['ngayTao']),
      parentCommentId: json['parentCommentId'],
      userId: json['userId'] ?? '',
      userName: json['userName'] ?? 'Unknown',
      userAvatar: json['userAvatar'],
    );
  }
}
```

### 2. Lấy bình luận của người dùng khác (Public)

**Endpoint:** `GET /api/Post/public/{userId}/comments`  
**Auth:** ❌ Không yêu cầu  

**Dart Code:**
```dart
Future<List<Comment>> getPublicUserComments({
  required String userId,
  required int offset,
  int limit = 10,
}) async {
  final url = Uri.parse(
    '$baseUrl/api/Post/public/$userId/comments?offset=$offset&limit=$limit'
  );
  
  final response = await http.get(url);
  
  if (response.statusCode == 200) {
    final data = jsonDecode(response.body);
    return (data['data']['comments'] as List)
        .map((c) => Comment.fromJson(c))
        .toList();
  }
  throw Exception('Lỗi tải bình luận');
}
```

### 3. Thêm bình luận

**Endpoint:** `POST /api/Post/{postId}/comments`  
**Auth:** ✅ Yêu cầu đăng nhập  
**Request Body:**
```json
{
  "noiDung": "Nội dung bình luận...",
  "parentCommentId": null
}
```

**Dart Code:**
```dart
Future<Comment> addComment({
  required String postId,
  required String noiDung,
  String? parentCommentId,
}) async {
  final url = Uri.parse('$baseUrl/api/Post/$postId/comments');
  
  final response = await http.post(
    url,
    headers: {
      'Authorization': 'Bearer $token',
      'Content-Type': 'application/json',
    },
    body: jsonEncode({
      'noiDung': noiDung,
      'parentCommentId': parentCommentId,
    }),
  );

  if (response.statusCode == 201) {
    final data = jsonDecode(response.body);
    return Comment.fromJson(data['data']);
  }
  throw Exception('Lỗi thêm bình luận');
}
```

---

## 👤 User Activity API

### Lấy bài viết yêu thích của bạn

**Endpoint:** `GET /api/Post/user/likes`  
**Auth:** ✅ Yêu cầu đăng nhập  

**Dart Code:**
```dart
Future<List<Post>> getUserLikedPosts({required int offset, int limit = 10}) async {
  final url = Uri.parse('$baseUrl/api/Post/user/likes?offset=$offset&limit=$limit');
  
  final response = await http.get(
    url,
    headers: {'Authorization': 'Bearer $token'},
  );

  if (response.statusCode == 200) {
    final data = jsonDecode(response.body);
    return (data['data']['posts'] as List)
        .map((p) => Post.fromJson(p))
        .toList();
  }
  throw Exception('Lỗi tải bài viết yêu thích');
}
```

---

## 🎯 Complete API Service Class

```dart
import 'package:http/http.dart' as http;
import 'dart:convert';

class HotelApiService {
  static const String baseUrl = 'https://localhost:7135';
  static String? _token;

  static void setToken(String token) {
    _token = token;
  }

  static Map<String, String> get _headers => {
    'Authorization': 'Bearer $_token',
    'Content-Type': 'application/json',
  };

  // ============= POSTS =============
  
  static Future<Map<String, dynamic>> getUserPosts({
    required int offset,
    int limit = 10,
  }) async {
    final url = Uri.parse('$baseUrl/api/Post/user/posts?offset=$offset&limit=$limit');
    final response = await http.get(url, headers: _headers);
    
    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    }
    throw Exception('Failed to load posts');
  }

  static Future<Map<String, dynamic>> getPublicUserPosts({
    required String userId,
    required int offset,
    int limit = 10,
  }) async {
    final url = Uri.parse(
      '$baseUrl/api/Post/public/$userId/posts?offset=$offset&limit=$limit'
    );
    final response = await http.get(url);
    
    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    }
    throw Exception('Failed to load public posts');
  }

  // ============= LIKES =============
  
  static Future<Map<String, dynamic>> toggleLike(String postId) async {
    final url = Uri.parse('$baseUrl/api/Post/$postId/like');
    final response = await http.post(url, headers: _headers);
    
    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    }
    throw Exception('Failed to toggle like');
  }

  static Future<Map<String, dynamic>> getUserLikedPosts({
    required int offset,
    int limit = 10,
  }) async {
    final url = Uri.parse('$baseUrl/api/Post/user/likes?offset=$offset&limit=$limit');
    final response = await http.get(url, headers: _headers);
    
    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    }
    throw Exception('Failed to load liked posts');
  }

  // ============= COMMENTS =============
  
  static Future<Map<String, dynamic>> getUserComments({
    required int offset,
    int limit = 10,
  }) async {
    final url = Uri.parse('$baseUrl/api/Post/user/comments?offset=$offset&limit=$limit');
    final response = await http.get(url, headers: _headers);
    
    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    }
    throw Exception('Failed to load comments');
  }

  static Future<Map<String, dynamic>> getPublicUserComments({
    required String userId,
    required int offset,
    int limit = 10,
  }) async {
    final url = Uri.parse(
      '$baseUrl/api/Post/public/$userId/comments?offset=$offset&limit=$limit'
    );
    final response = await http.get(url);
    
    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    }
    throw Exception('Failed to load public comments');
  }

  static Future<Map<String, dynamic>> addComment({
    required String postId,
    required String noiDung,
    String? parentCommentId,
  }) async {
    final url = Uri.parse('$baseUrl/api/Post/$postId/comments');
    final response = await http.post(
      url,
      headers: _headers,
      body: jsonEncode({
        'noiDung': noiDung,
        'parentCommentId': parentCommentId,
      }),
    );
    
    if (response.statusCode == 201 || response.statusCode == 200) {
      return jsonDecode(response.body);
    }
    throw Exception('Failed to add comment');
  }
}
```

---

## 🚀 Thêm package vào pubspec.yaml

```yaml
dependencies:
  http: ^1.1.0
  intl: ^0.19.0
```

---

## 📊 HTTP Status Codes

| Code | Ý nghĩa |
|------|---------|
| 200 | ✅ Thành công |
| 201 | ✅ Tạo thành công |
| 400 | ❌ Request không hợp lệ |
| 401 | ❌ Cần đăng nhập |
| 403 | ❌ Không có quyền |
| 404 | ❌ Không tìm thấy |
| 500 | ❌ Lỗi server |

---

## 💡 Tips & Best Practices

### 1. Lưu Token Safely
```dart
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

final storage = FlutterSecureStorage();

// Lưu token
await storage.write(key: 'jwt_token', value: token);

// Lấy token
final token = await storage.read(key: 'jwt_token');

// Xóa token (logout)
await storage.delete(key: 'jwt_token');
```

### 2. Handle Errors
```dart
try {
  final posts = await HotelApiService.getUserPosts(offset: 0);
} on SocketException {
  print('Lỗi kết nối');
} on TimeoutException {
  print('Request timeout');
} catch (e) {
  print('Lỗi: $e');
}
```

### 3. Pagination State Management
```dart
class PostsNotifier extends StateNotifier<List<Post>> {
  int _offset = 0;
  bool _hasMore = true;

  Future<void> loadMore() async {
    if (!_hasMore) return;
    
    final data = await HotelApiService.getUserPosts(offset: _offset);
    final posts = (data['data']['posts'] as List)
        .map((p) => Post.fromJson(p))
        .toList();
    
    state = [...state, ...posts];
    _offset += posts.length;
    _hasMore = data['data']['hasMore'] ?? false;
  }
}
```

---

## 📝 Response Format

Tất cả response đều tuân theo format này:

```json
{
  "success": true|false,
  "message": "Mô tả",
  "data": { /* Dữ liệu */ },
  "errors": [ /* Danh sách lỗi */ ]
}
```

---

## 🔗 Base URL

- **Development**: `https://localhost:7135`
- **Production**: `https://api.example.com`

---

## ❓ Câu hỏi thường gặp

**Q: Làm sao load infinite scroll?**  
A: Dùng `offset` + `limit`, mỗi lần scroll đến bottom, tăng `offset` thêm `limit`

**Q: Token hết hạn sao?**  
A: Gọi login lại để lấy token mới hoặc implement refresh token

**Q: Hỗ trợ filter/search không?**  
A: Hiện tại chưa, liên hệ backend team để thêm

**Q: Lấy comment của 1 bài viết?**  
A: Dùng endpoint: `GET /api/Post/{postId}/comments`

---

**Last Updated:** 17/11/2025  
**API Version:** 1.0  
**Contact:** backend@example.com
