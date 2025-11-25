# Hướng dẫn sử dụng API Upload Ảnh Base64

## Thay đổi chính
API upload ảnh đã được chuyển đổi từ multipart file upload sang Base64. Thay vì upload file trực tiếp vào thư mục, ảnh được gửi dưới dạng Base64 string.

### Lợi ích của Base64
- ✅ Không cần thư mục `/uploads` trên server
- ✅ Có thể lưu trực tiếp vào database nếu cần
- ✅ Dễ dàng truyền ảnh qua API
- ✅ Hỗ trợ tốt trên mobile app (Flutter)
- ✅ Có thể lưu ảnh trong JSON response
- ✅ Xác minh magic bytes của hình ảnh

## Endpoint Upload Ảnh

### URL
```
POST /api/post/upload
```

### Authentication
Yêu cầu JWT Bearer Token

### Request Format
```json
{
  "imageBase64": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUA...",
  "fileName": "my-image.png"
}
```

**Hoặc chỉ Base64 thuần (không có prefix):**
```json
{
  "imageBase64": "iVBORw0KGgoAAAANSUhEUgAAAAUA...",
  "fileName": "my-image.png"
}
```

### Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `imageBase64` | string | Yes | Base64 encoded image. Hỗ trợ format `data:image/*;base64,...` hoặc chỉ Base64 thuần |
| `fileName` | string | Yes | Tên file ảnh. Phần mở rộng phải là: `.jpg`, `.jpeg`, `.png`, `.gif`, hoặc `.webp` |

### Validation Rules

✅ **Kích thước**: Tối đa 5MB  
✅ **Format ảnh**: JPG, JPEG, PNG, GIF, WebP  
✅ **Magic bytes**: Được kiểm tra để đảm bảo file là ảnh hợp lệ  
✅ **Base64 format**: Phải là Base64 hợp lệ  

### Response Success (200 OK)
```json
{
  "success": true,
  "message": "Upload ảnh thành công",
  "data": {
    "fileName": "my-image.png",
    "imageBase64": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUA...",
    "size": 12345,
    "mimeType": "image/png"
  }
}
```

### Response Errors

**400 Bad Request - Ảnh không hợp lệ**
```json
{
  "success": false,
  "message": "Ảnh không hợp lệ. Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif, webp) với kích thước tối đa 5MB",
  "data": null,
  "errors": []
}
```

**401 Unauthorized - Chưa đăng nhập**
```json
{
  "success": false,
  "message": "Bạn cần đăng nhập để upload ảnh",
  "data": null,
  "errors": []
}
```

## Cách sử dụng trên Flutter

### Cách 1: Sử dụng file từ image picker
```dart
import 'dart:convert';
import 'dart:io';
import 'package:image_picker/image_picker.dart';
import 'package:http/http.dart' as http;

Future<void> uploadImage() async {
  final picker = ImagePicker();
  final pickedFile = await picker.pickImage(source: ImageSource.gallery);
  
  if (pickedFile != null) {
    // Đọc file và chuyển thành Base64
    final imageFile = File(pickedFile.path);
    final imageBytes = await imageFile.readAsBytes();
    final base64Image = base64Encode(imageBytes);
    
    // Tạo request
    final response = await http.post(
      Uri.parse('https://your-api.com/api/post/upload'),
      headers: {
        'Authorization': 'Bearer $token',
        'Content-Type': 'application/json',
      },
      body: jsonEncode({
        'imageBase64': base64Image,
        'fileName': pickedFile.name,
      }),
    );
    
    if (response.statusCode == 200) {
      final result = jsonDecode(response.body);
      print('Upload thành công: ${result['data']['imageBase64']}');
    }
  }
}
```

### Cách 2: Sử dụng data URL (với prefix)
```dart
// Nếu bạn đã có Base64 từ các nguồn khác
final response = await http.post(
  Uri.parse('https://your-api.com/api/post/upload'),
  headers: {
    'Authorization': 'Bearer $token',
    'Content-Type': 'application/json',
  },
  body: jsonEncode({
    'imageBase64': 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUA...',
    'fileName': 'image.png',
  }),
);
```

### Cách 3: Tạo bài viết với ảnh Base64

```dart
Future<void> createPostWithImage() async {
  final picker = ImagePicker();
  final pickedFile = await picker.pickImage(source: ImageSource.gallery);
  
  if (pickedFile != null) {
    // Upload ảnh
    final imageBytes = await File(pickedFile.path).readAsBytes();
    final base64Image = base64Encode(imageBytes);
    
    final uploadResponse = await http.post(
      Uri.parse('https://your-api.com/api/post/upload'),
      headers: {
        'Authorization': 'Bearer $token',
        'Content-Type': 'application/json',
      },
      body: jsonEncode({
        'imageBase64': base64Image,
        'fileName': pickedFile.name,
      }),
    );
    
    if (uploadResponse.statusCode == 200) {
      final uploadResult = jsonDecode(uploadResponse.body);
      final imageBase64 = uploadResult['data']['imageBase64'];
      
      // Tạo bài viết với ảnh
      final postResponse = await http.post(
        Uri.parse('https://your-api.com/api/post'),
        headers: {
          'Authorization': 'Bearer $token',
          'Content-Type': 'application/json',
        },
        body: jsonEncode({
          'noiDung': 'Bài viết của tôi',
          'loai': 'post',
          'duongDanMedia': imageBase64, // Sử dụng Base64 từ response upload
          'hashtags': '#food #health',
        }),
      );
      
      if (postResponse.statusCode == 201) {
        print('Bài viết tạo thành công!');
      }
    }
  }
}
```

## Cách sử dụng trên Web

### Cách 1: Sử dụng JavaScript
```javascript
async function uploadImage(file) {
  const reader = new FileReader();
  
  reader.onload = async function(e) {
    const base64String = e.target.result.split(',')[1];
    
    const response = await fetch('https://your-api.com/api/post/upload', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        imageBase64: base64String,
        fileName: file.name,
      }),
    });
    
    const result = await response.json();
    console.log('Upload thành công:', result.data.imageBase64);
  };
  
  reader.readAsDataURL(file);
}

// Sử dụng
const fileInput = document.getElementById('imageInput');
uploadImage(fileInput.files[0]);
```

### Cách 2: Sử dụng React với preview
```jsx
import React, { useState } from 'react';

function ImageUpload() {
  const [preview, setPreview] = useState(null);
  const [uploading, setUploading] = useState(false);
  
  const handleImageChange = (e) => {
    const file = e.target.files[0];
    const reader = new FileReader();
    
    reader.onload = async (event) => {
      const base64 = event.target.result.split(',')[1];
      setPreview(event.target.result);
      
      // Upload
      setUploading(true);
      const response = await fetch('/api/post/upload', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          imageBase64: base64,
          fileName: file.name,
        }),
      });
      
      const result = await response.json();
      setUploading(false);
      
      if (result.success) {
        console.log('Upload thành công:', result.data);
      }
    };
    
    reader.readAsDataURL(file);
  };
  
  return (
    <div>
      <input 
        type="file" 
        accept="image/*" 
        onChange={handleImageChange}
        disabled={uploading}
      />
      {preview && <img src={preview} alt="preview" style={{maxWidth: '200px'}} />}
      {uploading && <p>Đang upload...</p>}
    </div>
  );
}

export default ImageUpload;
```

## Lưu ý quan trọng

1. **Kích thước Base64**: Base64 string sẽ lớn hơn file gốc khoảng 33% do encoding
2. **Bandwidth**: Base64 tiêu tốn nhiều băng thông hơn file binary
3. **Storage**: Nếu lưu ảnh dưới dạng Base64 trong database, sẽ tiêu tốn nhiều không gian hơn
4. **Magic bytes**: API kiểm tra magic bytes để đảm bảo file thực sự là ảnh

## Khuyến nghị

- **Cho mobile (Flutter)**: ✅ Sử dụng Base64, có thể lưu vào database local
- **Cho web**: ✅ Sử dụng Base64, có thể hiển thị preview ngay
- **Cho high-traffic server**: ⚠️ Cân nhắc lưu ảnh vào object storage (AWS S3, Azure Blob, etc.) để tiết kiệm không gian database

## Chuyển đổi cơ sở dữ liệu

Nếu muốn lưu ảnh Base64 vào database, thêm cột vào model:

```csharp
public class BaiDang
{
    public Guid Id { get; set; }
    public string? DuongDanMedia { get; set; }  // URL hoặc Base64
    public byte[]? ImageData { get; set; }      // (Optional) Dữ liệu ảnh binary
    // ... các thuộc tính khác
}
```

Hoặc chỉ lưu Base64 string trực tiếp vào `DuongDanMedia` nếu là `NVARCHAR(MAX)`.
