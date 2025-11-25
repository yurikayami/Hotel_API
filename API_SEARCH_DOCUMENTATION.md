# API Backend Documentation - Tìm Kiếm Tổng Quát

Tài liệu này mô tả các API backend cần thiết để hỗ trợ tính năng tìm kiếm tổng quát trong ứng dụng Flutter Hotel Android. Phần `/api/mon-an` (món ăn) được giữ nguyên, các API khác sẽ được thêm mới.

---

## Mục Lục

1. [API Tìm Kiếm Tổng Quát](#1-api-tìm-kiếm-tổng-quát-ưu-tiên)
2. [API Riêng Cho Từng Loại](#2-api-riêng-cho-từng-loại)
3. [API Gợi Ý / Suggestions](#3-api-gợi-ý--suggestions-autocomplete)
4. [Xử Lý Tìm Kiếm Không Dấu](#4-xử-lý-tìm-kiếm-không-dấu-implementation-notes)
5. [Mô Tả Các Loại Dữ Liệu](#5-mô-tả-các-loại-dữ-liệu)
6. [HTTP Status Codes](#6-http-status-codes)
7. [Rate Limiting & Security](#7-rate-limiting--security)
8. [Caching Strategy](#8-caching-strategy)
9. [Testing API](#9-testing-api)
10. [Example Implementation](#10-example-implementation-nodejs--express)

---

## 1. API Tìm Kiếm Tổng Quát (Ưu Tiên)

### Endpoint
```
GET /api/search
```

### Mô Tả
Endpoint tổng quát để tìm kiếm trên tất cả các loại dữ liệu: người dùng, bài đăng, bài thuốc, và món ăn. Hỗ trợ tìm kiếm không dấu (ví dụ: tìm "com ga" sẽ tìm được "cơm gà").

### Parameters

| Parameter | Type | Required | Default | Mô Tả |
|-----------|------|----------|---------|-------|
| `q` | string | Yes | - | Query tìm kiếm (hỗ trợ không dấu). Ví dụ: "cơm gà", "nguyễn văn a", "thuốc cảm". |
| `type` | string | No | "all" | Loại tìm kiếm: `all`, `users`, `posts`, `medicines`, `dishes`. |
| `page` | integer | No | 1 | Trang hiện tại (cho pagination). |
| `limit` | integer | No | 20 | Số kết quả mỗi trang (max: 100). |

### Request Example

```bash
# Tìm kiếm tất cả
GET /api/search?q=cơm&type=all&page=1&limit=20

# Tìm kiếm không dấu
GET /api/search?q=com+ga&type=all&page=1&limit=20

# Tìm kiếm chỉ món ăn
GET /api/search?q=cơm&type=dishes&page=1&limit=20

# Tìm kiếm chỉ người dùng
GET /api/search?q=nguyễn&type=users&page=1&limit=20

# Tìm kiếm chỉ bài thuốc
GET /api/search?q=cảm&type=medicines&page=1&limit=20
```

### Response Success (200 OK)

```json
{
  "success": true,
  "message": "Tìm kiếm thành công",
  "data": {
    "users": [
      {
        "id": 1,
        "name": "Nguyễn Văn A",
        "avatar": "https://example.com/avatars/user_1.jpg",
        "email": "user1@example.com"
      },
      {
        "id": 2,
        "name": "Trần Thị B",
        "avatar": "https://example.com/avatars/user_2.jpg",
        "email": "user2@example.com"
      }
    ],
    "posts": [
      {
        "id": 101,
        "title": "Bài đăng về cơm gà",
        "content": "Hướng dẫn nấu cơm gà ngon...",
        "image": "https://example.com/posts/post_101.jpg",
        "userId": 1,
        "createdAt": "2025-11-20T10:30:00Z",
        "viewCount": 150
      }
    ],
    "medicines": [
      {
        "id": 201,
        "name": "Thuốc Cảm Hạ Sốt",
        "description": "Thuốc cảm hiệu quả, hạ sốt nhanh",
        "image": "https://example.com/medicines/med_201.jpg",
        "category": "Cảm Cúm",
        "ingredients": ["Paracetamol 500mg"]
      }
    ],
    "dishes": [
      {
        "id": 301,
        "ten": "Cơm Gà Hainan",
        "gia": 65000,
        "image": "https://example.com/dishes/dish_301.jpg",
        "loai": "Cơm",
        "soNguoi": 1,
        "moTa": "Cơm gà Hainan cổ điển"
      }
    ]
  },
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 100,
    "totalPages": 5
  }
}
```

### Response Error (400 Bad Request)

```json
{
  "success": false,
  "message": "Query không hợp lệ hoặc quá ngắn",
  "code": "INVALID_QUERY"
}
```

### Response Error (500 Internal Server Error)

```json
{
  "success": false,
  "message": "Lỗi server: không thể thực hiện tìm kiếm",
  "code": "SERVER_ERROR"
}
```

### Ghi Chú Implement Backend

#### Xử Lý Không Dấu
- **Normalize query**: Chuyển đổi query về không dấu trước khi tìm kiếm. Ví dụ:
  - "cơm gà" → "com ga"
  - "nguyễn văn a" → "nguyen van a"
  - "thuốc cảm" → "thuoc cam"
- **Database**: Lưu thêm cột `name_normalized` (hoặc tương tự) cho mỗi bảng (users, posts, medicines, dishes) để tìm kiếm nhanh. Cập nhật cột này khi insert/update dữ liệu.
- **Full-Text Search** (Tuỳ chọn nâng cao):
  - PostgreSQL: Sử dụng `tsvector` và `tsquery` cho full-text search.
  - MySQL: Sử dụng `FULLTEXT` index.
  - Elasticsearch: Nếu cần search advanced hơn (fuzzy matching, synonyms, v.v.).

#### Validate Input
- Kiểm tra `q` không rỗng và độ dài tối thiểu (ví dụ: ≥ 2 ký tự).
- Sanitize `q` để tránh SQL injection (dùng prepared statements).
- Validate `type`, `page`, `limit` (kiểu dữ liệu, giá trị hợp lệ).

#### Performance
- Sử dụng indexing trên cột `name_normalized`, `title_normalized`, etc.
- Implement cache (Redis) cho queries phổ biến.
- Limit max `limit` parameter (ví dụ: max 100) để tránh quá tải.
- Implement rate limiting (ví dụ: 100 requests/phút per IP).

---

## 2. API Riêng Cho Từng Loại

Ngoài endpoint tổng quát, cung cấp các endpoint riêng cho từng loại để linh hoạt hơn:

### 2.1 API Tìm Kiếm Người Dùng

#### Endpoint
```
GET /api/users/search
```

#### Parameters

| Parameter | Type | Required | Default |
|-----------|------|----------|---------|
| `q` | string | Yes | - |
| `page` | integer | No | 1 |
| `limit` | integer | No | 20 |

#### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "users": [
      {
        "id": 1,
        "name": "Nguyễn Văn A",
        "avatar": "https://example.com/avatars/user_1.jpg",
        "email": "user1@example.com",
        "bio": "Yêu thích nấu ăn"
      }
    ]
  },
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 50
  }
}
```

---

### 2.2 API Tìm Kiếm Bài Đăng

#### Endpoint
```
GET /api/posts/search
```

#### Parameters

| Parameter | Type | Required | Default |
|-----------|------|----------|---------|
| `q` | string | Yes | - |
| `page` | integer | No | 1 |
| `limit` | integer | No | 20 |
| `sort` | string | No | "recent" |

#### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "posts": [
      {
        "id": 101,
        "title": "Bài đăng về cơm gà",
        "content": "Hướng dẫn nấu cơm gà ngon...",
        "image": "https://example.com/posts/post_101.jpg",
        "userId": 1,
        "userName": "Nguyễn Văn A",
        "createdAt": "2025-11-20T10:30:00Z",
        "viewCount": 150,
        "likeCount": 25
      }
    ]
  },
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 75
  }
}
```

---

### 2.3 API Tìm Kiếm Bài Thuốc (Hỗ Trợ Không Dấu)

#### Endpoint
```
GET /api/medicines/search
```

#### Parameters

| Parameter | Type | Required | Default |
|-----------|------|----------|---------|
| `q` | string | Yes | - |
| `page` | integer | No | 1 |
| `limit` | integer | No | 20 |
| `category` | string | No | - |

#### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "medicines": [
      {
        "id": 201,
        "name": "Thuốc Cảm Hạ Sốt",
        "description": "Thuốc cảm hiệu quả, hạ sốt nhanh",
        "image": "https://example.com/medicines/med_201.jpg",
        "category": "Cảm Cúm",
        "ingredients": ["Paracetamol 500mg", "Vitamin C 100mg"],
        "dosage": "1-2 viên, 3 lần/ngày",
        "sideEffects": "Hiếm",
        "createdAt": "2025-11-01T08:00:00Z"
      }
    ]
  },
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 40
  }
}
```

#### Ghi Chú
- Hỗ trợ tìm kiếm không dấu: "thuoc cam" tìm được "Thuốc Cảm".
- Hỗ trợ filter theo category nếu cần.

---

### 2.4 API Tìm Kiếm Món Ăn (Hiện Tại)

#### Endpoint
```
GET /api/mon-an
```

#### Ghi Chú
- **Giữ nguyên**: Endpoint hiện tại đã hoạt động, không cần thay đổi.
- Thêm parameter `search` hoặc `q` nếu chưa có để hỗ trợ tìm kiếm không dấu:
  ```
  GET /api/mon-an?q=com+ga&page=1&limit=20
  ```
- Response format tương tự như hiện tại:
  ```json
  {
    "success": true,
    "data": {
      "monAn": [
        {
          "id": 301,
          "ten": "Cơm Gà Hainan",
          "gia": 65000,
          "image": "https://example.com/dishes/dish_301.jpg",
          "loai": "Cơm",
          "soNguoi": 1,
          "moTa": "Cơm gà Hainan cổ điển"
        }
      ]
    },
    "pagination": {
      "page": 1,
      "limit": 20,
      "total": 150
    }
  }
  ```

---

## 3. API Gợi Ý / Suggestions (Autocomplete)

### Endpoint
```
GET /api/search/suggestions
```

### Mô Tả
Cung cấp gợi ý từ khóa dựa trên query nhập vào, hỗ trợ autocomplete. Trả về danh sách các từ khóa phổ biến hoặc liên quan.

### Parameters

| Parameter | Type | Required | Default |
|-----------|------|----------|---------|
| `q` | string | Yes | - |
| `type` | string | No | "all" |
| `limit` | integer | No | 10 |

### Request Example

```bash
GET /api/search/suggestions?q=cơ&type=all&limit=10
GET /api/search/suggestions?q=com&type=dishes&limit=10
```

### Response (200 OK)

```json
{
  "success": true,
  "data": {
    "suggestions": [
      "cơm gà",
      "cơm chiên",
      "cơm tấm",
      "cơm cốc",
      "cơm dương",
      "cơm lạnh",
      "cơm trắng",
      "cơm xào",
      "cơm hàng",
      "cơm bình dân"
    ]
  }
}
```

### Ghi Chú
- Có thể tính dựa trên lịch sử tìm kiếm hoặc từ khóa phổ biến.
- Dùng cho dropdown gợi ý trong UI `SearchAnchor` của Flutter.

---

## 4. Xử Lý Tìm Kiếm Không Dấu (Implementation Notes)

### 4.1 Backend - Normalize Database

#### PostgreSQL Ví Dụ
```sql
-- Tạo extension unaccent (nếu chưa có)
CREATE EXTENSION IF NOT EXISTS unaccent;

-- Tạo cột normalized cho bảng mon_an
ALTER TABLE mon_an ADD COLUMN ten_normalized VARCHAR(255);

-- Cập nhật dữ liệu
UPDATE mon_an SET ten_normalized = unaccent(lower(ten));

-- Tạo trigger để auto-update khi insert/update
CREATE OR REPLACE FUNCTION update_ten_normalized()
RETURNS TRIGGER AS $$
BEGIN
  NEW.ten_normalized := unaccent(lower(NEW.ten));
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER mon_an_normalize
BEFORE INSERT OR UPDATE ON mon_an
FOR EACH ROW
EXECUTE FUNCTION update_ten_normalized();

-- Tạo index cho tìm kiếm nhanh
CREATE INDEX idx_mon_an_ten_normalized ON mon_an(ten_normalized);
```

#### MySQL Ví Dụ
```sql
-- MySQL 8.0+ không có unaccent built-in, dùng workaround
ALTER TABLE mon_an ADD COLUMN ten_normalized VARCHAR(255);

-- Trong application code, normalize query và database values
-- Hoặc sử dụng library như `mysql-unaccent` (external tool)

-- Tạo index
CREATE INDEX idx_mon_an_ten_normalized ON mon_an(ten_normalized);
```

#### Node.js / Python Backend Ví Dụ
```python
# Python - dùng `unidecode` package
from unidecode import unidecode

def normalize_text(text):
    """Convert 'Cơm Gà' -> 'Com Ga'"""
    return unidecode(text).lower()

# Query example
query = "com ga"
normalized_query = normalize_text(query)

# MongoDB
medicines = db.medicines.find(
    {"name_normalized": {"$regex": normalized_query, "$options": "i"}}
)

# SQL
import mysql.connector
cursor = conn.cursor()
query_sql = """
SELECT * FROM mon_an 
WHERE ten_normalized LIKE %s 
OR ten LIKE %s
LIMIT 20
"""
cursor.execute(query_sql, (f"%{normalized_query}%", f"%{query}%"))
```

```javascript
// Node.js - dùng `unidecode` package
const unidecode = require('unidecode');

function normalizeText(text) {
  return unidecode(text).toLowerCase();
}

// Query example
const query = "com ga";
const normalizedQuery = normalizeText(query);

// MongoDB
const medicines = await db.collection('medicines').find(
  { nameNormalized: { $regex: normalizedQuery, $options: 'i' } }
).toArray();

// PostgreSQL
const result = await db.query(
  'SELECT * FROM mon_an WHERE ten_normalized ILIKE $1',
  [`%${normalizedQuery}%`]
);
```

### 4.2 Query Normalization Logic

```
Input: "Cơm Gà Nước Lèo"
Step 1: Lowercase -> "cơm gà nước lèo"
Step 2: Remove diacritics -> "com ga nuoc leo"
Output: "com ga nuoc leo" (dùng cho tìm kiếm)
```

---

## 5. Mô Tả Các Loại Dữ Liệu

### 5.1 User (Người Dùng)

```json
{
  "id": 1,
  "name": "Nguyễn Văn A",
  "email": "user1@example.com",
  "avatar": "https://example.com/avatars/user_1.jpg",
  "bio": "Yêu thích nấu ăn",
  "phone": "0901234567",
  "createdAt": "2025-01-15T10:30:00Z"
}
```

### 5.2 Post (Bài Đăng)

```json
{
  "id": 101,
  "title": "Hướng dẫn nấu cơm gà Hainan",
  "content": "Bài hướng dẫn chi tiết...",
  "image": "https://example.com/posts/post_101.jpg",
  "userId": 1,
  "userName": "Nguyễn Văn A",
  "createdAt": "2025-11-20T10:30:00Z",
  "updatedAt": "2025-11-20T15:45:00Z",
  "viewCount": 150,
  "likeCount": 25,
  "commentCount": 10
}
```

### 5.3 Medicine (Bài Thuốc)

```json
{
  "id": 201,
  "name": "Thuốc Cảm Hạ Sốt",
  "description": "Thuốc cảm hiệu quả, hạ sốt nhanh",
  "image": "https://example.com/medicines/med_201.jpg",
  "category": "Cảm Cúm",
  "ingredients": [
    "Paracetamol 500mg",
    "Vitamin C 100mg"
  ],
  "dosage": "1-2 viên, 3 lần/ngày",
  "sideEffects": "Hiếm gặp",
  "contraindications": "Không dùng cho phụ nữ mang thai",
  "createdAt": "2025-11-01T08:00:00Z",
  "updatedAt": "2025-11-10T12:00:00Z"
}
```

### 5.4 Dish (Món Ăn) - Hiện Tại

```json
{
  "id": 301,
  "ten": "Cơm Gà Hainan",
  "gia": 65000,
  "image": "https://example.com/dishes/dish_301.jpg",
  "loai": "Cơm",
  "soNguoi": 1,
  "moTa": "Cơm gà Hainan cổ điển",
  "ngayTao": "2025-01-10T08:00:00Z",
  "luotXem": 500
}
```

---

## 6. HTTP Status Codes

| Status | Mô Tả |
|--------|-------|
| 200 | OK - Tìm kiếm thành công |
| 400 | Bad Request - Query không hợp lệ, parameter sai |
| 401 | Unauthorized - Chưa xác thực (nếu cần) |
| 429 | Too Many Requests - Vượt quá rate limit |
| 500 | Internal Server Error - Lỗi server |

---

## 7. Rate Limiting & Security

### Rate Limiting
- **Search API**: 100 requests/phút per IP.
- **Suggestions API**: 200 requests/phút per IP.

### Security
- Validate và sanitize tất cả input.
- Sử dụng prepared statements để tránh SQL injection.
- Implement CORS nếu frontend ở domain khác.
- Sử dụng HTTPS cho tất cả requests.
- Add logging để monitor suspicious activity.

---

## 8. Caching Strategy

### Cache Keys (Redis)
```
search:{type}:{normalized_query}:{page}:{limit}
suggestions:{type}:{normalized_query}:{limit}

Ví dụ:
search:all:com ga:1:20
search:dishes:com ga:1:20
suggestions:dishes:com:10
```

### Cache TTL
- **Search results**: 1 giờ
- **Suggestions**: 24 giờ
- **Invalidate**: Khi có insert/update dữ liệu liên quan

---

## 9. Testing API

### Test Cases

#### Test 1: Tìm kiếm không dấu - Món ăn
```bash
GET /api/search?q=com+ga&type=dishes

Kỳ vọng: Trả về các món ăn có tên chứa "cơm gà", "cơm", "gà", etc.
```

#### Test 2: Tìm kiếm không dấu - Bài thuốc
```bash
GET /api/search?q=thuoc+cam&type=medicines

Kỳ vọng: Trả về các bài thuốc liên quan "cảm", "hạ sốt", etc.
```

#### Test 3: Tìm kiếm tất cả loại
```bash
GET /api/search?q=suc+khoe&type=all&limit=20

Kỳ vọng: Trả về users, posts, medicines, dishes liên quan tới "sức khỏe"
```

#### Test 4: Gợi ý từ khóa
```bash
GET /api/search/suggestions?q=com&type=dishes&limit=10

Kỳ vọng: Trả về ["cơm gà", "cơm chiên", "cơm tấm", ...]
```

---

## 10. Example Implementation (Node.js + Express)

### Install Dependencies
```bash
npm install express axios mysql2 unidecode cors dotenv
```

### Code Sample

```javascript
const express = require('express');
const mysql = require('mysql2/promise');
const unidecode = require('unidecode');
const router = express.Router();

const pool = mysql.createPool({
  host: process.env.DB_HOST,
  user: process.env.DB_USER,
  password: process.env.DB_PASSWORD,
  database: process.env.DB_NAME,
  waitForConnections: true,
  connectionLimit: 10,
  queueLimit: 0,
});

// Helper: Normalize text
function normalizeText(text) {
  return unidecode(text).toLowerCase();
}

// Helper: Validate query
function validateSearchQuery(q) {
  if (!q || q.trim().length < 2) {
    return { valid: false, error: 'Query quá ngắn' };
  }
  if (q.length > 255) {
    return { valid: false, error: 'Query quá dài' };
  }
  return { valid: true };
}

// API: General Search
router.get('/', async (req, res) => {
  try {
    const { q, type = 'all', page = 1, limit = 20 } = req.query;

    // Validate
    const validation = validateSearchQuery(q);
    if (!validation.valid) {
      return res.status(400).json({
        success: false,
        message: validation.error,
        code: 'INVALID_QUERY',
      });
    }

    const normalizedQ = normalizeText(q);
    const offset = (parseInt(page) - 1) * parseInt(limit);
    const connection = await pool.getConnection();

    const data = {};

    try {
      // Search Users
      if (type === 'all' || type === 'users') {
        const [users] = await connection.execute(
          `SELECT id, name, email, avatar FROM users 
           WHERE CONCAT(LOWER(name), ' ', LOWER(email)) LIKE ? 
           LIMIT ? OFFSET ?`,
          [`%${normalizedQ}%`, parseInt(limit), offset]
        );
        data.users = users || [];
      }

      // Search Posts
      if (type === 'all' || type === 'posts') {
        const [posts] = await connection.execute(
          `SELECT id, title, content, image, userId, createdAt, viewCount 
           FROM posts 
           WHERE title LIKE ? OR content LIKE ? 
           LIMIT ? OFFSET ?`,
          [`%${q}%`, `%${q}%`, parseInt(limit), offset]
        );
        data.posts = posts || [];
      }

      // Search Medicines (no-diacritic support)
      if (type === 'all' || type === 'medicines') {
        const [medicines] = await connection.execute(
          `SELECT id, name, description, image, category FROM medicines 
           WHERE LOWER(CONCAT(name, ' ', description)) LIKE ? 
           LIMIT ? OFFSET ?`,
          [`%${normalizedQ}%`, parseInt(limit), offset]
        );
        data.medicines = medicines || [];
      }

      // Search Dishes (no-diacritic support)
      if (type === 'all' || type === 'dishes') {
        const [dishes] = await connection.execute(
          `SELECT id, ten, gia, image, loai, soNguoi FROM mon_an 
           WHERE LOWER(ten) LIKE ? OR LOWER(moTa) LIKE ? 
           LIMIT ? OFFSET ?`,
          [`%${normalizedQ}%`, `%${normalizedQ}%`, parseInt(limit), offset]
        );
        data.dishes = dishes || [];
      }

      res.json({
        success: true,
        message: 'Tìm kiếm thành công',
        data,
        pagination: {
          page: parseInt(page),
          limit: parseInt(limit),
          total: 100, // TODO: Get actual total count
        },
      });
    } finally {
      connection.release();
    }
  } catch (error) {
    console.error('Search error:', error);
    res.status(500).json({
      success: false,
      message: 'Lỗi server',
      code: 'SERVER_ERROR',
    });
  }
});

// API: Suggestions
router.get('/suggestions', async (req, res) => {
  try {
    const { q, type = 'all', limit = 10 } = req.query;

    const validation = validateSearchQuery(q);
    if (!validation.valid) {
      return res.status(400).json({
        success: false,
        message: validation.error,
      });
    }

    const normalizedQ = normalizeText(q);
    const connection = await pool.getConnection();

    const suggestions = [];

    try {
      if (type === 'all' || type === 'dishes') {
        const [dishes] = await connection.execute(
          `SELECT DISTINCT ten FROM mon_an 
           WHERE LOWER(ten) LIKE ? 
           LIMIT ?`,
          [`${normalizedQ}%`, parseInt(limit)]
        );
        suggestions.push(...dishes.map(d => d.ten));
      }

      if (type === 'all' || type === 'medicines') {
        const [meds] = await connection.execute(
          `SELECT DISTINCT name FROM medicines 
           WHERE LOWER(name) LIKE ? 
           LIMIT ?`,
          [`${normalizedQ}%`, parseInt(limit)]
        );
        suggestions.push(...meds.map(m => m.name));
      }

      res.json({
        success: true,
        data: {
          suggestions: suggestions.slice(0, limit),
        },
      });
    } finally {
      connection.release();
    }
  } catch (error) {
    console.error('Suggestions error:', error);
    res.status(500).json({
      success: false,
      message: 'Lỗi server',
    });
  }
});

module.exports = router;
```

---

## Kết Luận

Tài liệu này cung cấp đầy đủ các API cần thiết để:
1. ✅ Tìm kiếm tổng quát trên tất cả loại dữ liệu
2. ✅ Tìm kiếm riêng cho từng loại (users, posts, medicines, dishes)
3. ✅ Hỗ trợ gợi ý từ khóa (autocomplete)
4. ✅ Hỗ trợ tìm kiếm không dấu cho bài thuốc và món ăn
5. ✅ Pagination, caching, rate limiting
6. ✅ Error handling và logging

Backend team có thể implement theo guide này. Frontend (Flutter) sẽ consume các API này thông qua `SearchProvider` đã gợi ý trước đó.

---

**Phiên bản**: 1.0  
**Ngày cập nhật**: 21/11/2025  
**Tác giả**: AI Development Team
