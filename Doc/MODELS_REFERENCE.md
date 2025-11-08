# 🗂️ Models Reference - Hotel Web API

> Tài liệu mô tả các models/entities chính trong hệ thống để Flutter developers dễ dàng implement DTOs.

---

## 📋 Mục Lục

1. [User & Authentication](#1-user--authentication)
2. [Social Feed](#2-social-feed)
3. [Food & Menu](#3-food--menu)
4. [Order & Cart](#4-order--cart)
5. [Health Profile](#5-health-profile)
6. [Traditional Medicine](#6-traditional-medicine)
7. [Relationships](#7-relationships)

---

## 1. User & Authentication

### ApplicationUser

**Table**: `AspNetUsers`

```dart
class ApplicationUser {
  String id;
  String userName;
  String? email;
  String? phoneNumber;
  
  // Profile Info
  String? gioiTinh;           // "Male", "Female", "Other"
  int? tuoi;
  String? profilePicture;     // URL: "/images/avatar/..."
  String? displayName;
  String? avatarUrl;
  
  // Social Login
  bool? isFacebookLinked;
  bool? isGoogleLinked;
  String? googleProfilePicture;
  String? facebookProfilePicture;
  
  // Online Status
  bool? dangOnline;
  int? trangThai;             // 0: Offline, 1: Online, 2: Away
  DateTime? lanHoatDongCuoi;
  
  // For Doctors
  int? kinhNghiem;            // Years of experience
  String? chuyenKhoaId;       // Specialty ID
  
  ApplicationUser({
    required this.id,
    required this.userName,
    this.email,
    this.phoneNumber,
    this.gioiTinh,
    this.tuoi,
    this.profilePicture,
    this.displayName,
    this.avatarUrl,
    this.isFacebookLinked,
    this.isGoogleLinked,
    this.googleProfilePicture,
    this.facebookProfilePicture,
    this.dangOnline,
    this.trangThai,
    this.lanHoatDongCuoi,
    this.kinhNghiem,
    this.chuyenKhoaId,
  });
  
  factory ApplicationUser.fromJson(Map<String, dynamic> json) {
    return ApplicationUser(
      id: json['id'],
      userName: json['userName'],
      email: json['email'],
      phoneNumber: json['phoneNumber'],
      gioiTinh: json['gioi_tinh'],
      tuoi: json['tuoi'],
      profilePicture: json['profilePicture'],
      displayName: json['displayName'],
      avatarUrl: json['avatarUrl'],
      isFacebookLinked: json['isFacebookLinked'],
      isGoogleLinked: json['isGoogleLinked'],
      googleProfilePicture: json['googleProfilePicture'],
      facebookProfilePicture: json['facebookProfilePicture'],
      dangOnline: json['dang_online'],
      trangThai: json['trang_thai'],
      lanHoatDongCuoi: json['lan_hoat_dong_cuoi'] != null
          ? DateTime.parse(json['lan_hoat_dong_cuoi'])
          : null,
      kinhNghiem: json['kinh_nghiem'],
      chuyenKhoaId: json['chuyenKhoaId'],
    );
  }
}
```

---

## 2. Social Feed

### BaiDang (Post)

**Table**: `BaiDang`

```dart
class BaiDang {
  String id;                  // Guid
  String? nguoiDungId;
  String? noiDung;            // Post content
  String? loai;               // Post type
  String? duongDanMedia;      // Image/video URL
  DateTime? ngayDang;
  String? idMonAn;            // Optional: linked dish
  
  // Engagement
  int? luotThich;
  int? soBinhLuan;
  int soChiaSe;
  
  // Moderation
  bool? daDuyet;              // Approved by admin
  
  // SEO
  String? hashtags;           // JSON array of hashtags
  String? keywords;           // JSON array of keywords
  
  // Navigation (from API)
  ApplicationUser? author;
  MonAn? monAn;
  
  BaiDang({
    required this.id,
    this.nguoiDungId,
    this.noiDung,
    this.loai,
    this.duongDanMedia,
    this.ngayDang,
    this.idMonAn,
    this.luotThich,
    this.soBinhLuan,
    this.soChiaSe = 0,
    this.daDuyet,
    this.hashtags,
    this.keywords,
    this.author,
    this.monAn,
  });
  
  factory BaiDang.fromJson(Map<String, dynamic> json) {
    return BaiDang(
      id: json['id'],
      nguoiDungId: json['nguoiDungId'],
      noiDung: json['noiDung'],
      loai: json['loai'],
      duongDanMedia: json['duongDanMedia'],
      ngayDang: json['ngayDang'] != null
          ? DateTime.parse(json['ngayDang'])
          : null,
      idMonAn: json['id_MonAn'],
      luotThich: json['luotThich'] ?? 0,
      soBinhLuan: json['soBinhLuan'] ?? 0,
      soChiaSe: json['so_chia_se'] ?? 0,
      daDuyet: json['daDuyet'],
      hashtags: json['hashtags'],
      keywords: json['keywords'],
    );
  }
}
```

### FeedItemDto (Used in API Response)

```dart
class FeedItemDto {
  String id;
  String type;                // "Post" or "BaiThuoc"
  String? content;
  String? imageUrl;
  DateTime? ngayDang;
  int soBinhLuan;
  int soChiaSe;
  int? luotThich;
  bool isLiked;
  
  // Author info
  String? authorId;
  String? authorName;
  String? avartar;
  
  FeedItemDto({
    required this.id,
    required this.type,
    this.content,
    this.imageUrl,
    this.ngayDang,
    this.soBinhLuan = 0,
    this.soChiaSe = 0,
    this.luotThich = 0,
    this.isLiked = false,
    this.authorId,
    this.authorName,
    this.avartar,
  });
  
  factory FeedItemDto.fromJson(Map<String, dynamic> json) {
    return FeedItemDto(
      id: json['id'],
      type: json['type'],
      content: json['content'],
      imageUrl: json['imageUrl'],
      ngayDang: json['ngayDang'] != null
          ? DateTime.parse(json['ngayDang'])
          : null,
      soBinhLuan: json['soBinhLuan'] ?? 0,
      soChiaSe: json['soChiaSe'] ?? 0,
      luotThich: json['luotThich'] ?? 0,
      isLiked: json['isLiked'] ?? false,
      authorId: json['authorId'],
      authorName: json['authorName'],
      avartar: json['avartar'],
    );
  }
}
```

---

## 3. Food & Menu

### MonAn (Dish)

**Table**: `MonAn`

```dart
class MonAn {
  String id;
  String? ten;                // Dish name
  String? moTa;               // Description
  String? cachCheBien;        // Cooking instructions
  String? loai;               // Category: "Food", "Water", etc.
  DateTime? ngayTao;
  String? image;              // Image URL
  double? gia;                // Price (decimal 10,2)
  int? soNguoi;               // Serving size
  int luotXem;                // View count
  
  // Relations
  NutritionAnalyze? nutritionAnalyze;
  List<MonAnBenh>? monAnBenh; // Disease relationships
  
  MonAn({
    required this.id,
    this.ten,
    this.moTa,
    this.cachCheBien,
    this.loai,
    this.ngayTao,
    this.image,
    this.gia,
    this.soNguoi,
    this.luotXem = 0,
    this.nutritionAnalyze,
    this.monAnBenh,
  });
  
  factory MonAn.fromJson(Map<String, dynamic> json) {
    return MonAn(
      id: json['id'],
      ten: json['ten'],
      moTa: json['moTa'],
      cachCheBien: json['cachCheBien'],
      loai: json['loai'],
      ngayTao: json['ngayTao'] != null
          ? DateTime.parse(json['ngayTao'])
          : null,
      image: json['image'],
      gia: json['gia']?.toDouble(),
      soNguoi: json['soNguoi'],
      luotXem: json['luotXem'] ?? 0,
    );
  }
  
  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'ten': ten,
      'moTa': moTa,
      'cachCheBien': cachCheBien,
      'loai': loai,
      'ngayTao': ngayTao?.toIso8601String(),
      'image': image,
      'gia': gia,
      'soNguoi': soNguoi,
      'luotXem': luotXem,
    };
  }
}
```

### NutritionAnalyze

```dart
class NutritionAnalyze {
  String id;
  String monAnId;
  double? calories;
  double? protein;
  double? carbs;
  double? fat;
  double? fiber;
  double? sugar;
  double? sodium;
  
  NutritionAnalyze({
    required this.id,
    required this.monAnId,
    this.calories,
    this.protein,
    this.carbs,
    this.fat,
    this.fiber,
    this.sugar,
    this.sodium,
  });
  
  factory NutritionAnalyze.fromJson(Map<String, dynamic> json) {
    return NutritionAnalyze(
      id: json['id'],
      monAnId: json['monAnId'],
      calories: json['calories']?.toDouble(),
      protein: json['protein']?.toDouble(),
      carbs: json['carbs']?.toDouble(),
      fat: json['fat']?.toDouble(),
      fiber: json['fiber']?.toDouble(),
      sugar: json['sugar']?.toDouble(),
      sodium: json['sodium']?.toDouble(),
    );
  }
}
```

---

## 4. Order & Cart

### GioHang (Cart)

**Table**: `GioHang`

```dart
class GioHang {
  String id;
  String? nguoiDungId;
  DateTime ngayTao;
  DateTime ngayCapNhat;
  int? trangThai;             // 0: Active, 1: Checked out
  
  List<GioHangChiTiet> chiTiets;
  
  GioHang({
    required this.id,
    this.nguoiDungId,
    required this.ngayTao,
    required this.ngayCapNhat,
    this.trangThai,
    this.chiTiets = const [],
  });
  
  factory GioHang.fromJson(Map<String, dynamic> json) {
    return GioHang(
      id: json['id'],
      nguoiDungId: json['nguoiDungId'],
      ngayTao: DateTime.parse(json['ngayTao']),
      ngayCapNhat: DateTime.parse(json['ngayCapNhat']),
      trangThai: json['trangThai'],
      chiTiets: (json['chiTiets'] as List?)
          ?.map((e) => GioHangChiTiet.fromJson(e))
          .toList() ?? [],
    );
  }
}
```

### GioHangChiTiet (Cart Item)

**Table**: `GioHangChiTiet`

```dart
class GioHangChiTiet {
  String id;
  String gioHangId;
  String monAnId;
  int soLuong;
  double thanhTien;           // Total = DonGia * SoLuong
  
  MonAn? monAn;
  
  GioHangChiTiet({
    required this.id,
    required this.gioHangId,
    required this.monAnId,
    required this.soLuong,
    required this.thanhTien,
    this.monAn,
  });
  
  factory GioHangChiTiet.fromJson(Map<String, dynamic> json) {
    return GioHangChiTiet(
      id: json['id'],
      gioHangId: json['gioHangId'],
      monAnId: json['monAnId'],
      soLuong: json['soLuong'],
      thanhTien: json['thanhTien'].toDouble(),
      monAn: json['monAn'] != null 
          ? MonAn.fromJson(json['monAn']) 
          : null,
    );
  }
}
```

### CartItemDto (API Response)

```dart
class CartItemDto {
  String id;
  String monAnId;
  String tenMonAn;
  int soLuong;
  double? donGia;
  double thanhTien;
  String? imageUrl;
  
  CartItemDto({
    required this.id,
    required this.monAnId,
    required this.tenMonAn,
    required this.soLuong,
    this.donGia,
    required this.thanhTien,
    this.imageUrl,
  });
  
  factory CartItemDto.fromJson(Map<String, dynamic> json) {
    return CartItemDto(
      id: json['id'],
      monAnId: json['monAnId'],
      tenMonAn: json['tenMonAn'],
      soLuong: json['soLuong'],
      donGia: json['donGia']?.toDouble(),
      thanhTien: json['thanhTien'].toDouble(),
      imageUrl: json['imageUrl'],
    );
  }
}
```

### Order

**Table**: `Order`

```dart
class Order {
  String orderKey;            // Guid
  String orderId;             // Display ID
  DateTime orderDate;
  String guestName;
  int? reservationKey;
  String orderStatus;         // "Pending", "Confirmed", "Delivered", "Cancelled"
  String recordStatus;
  String createdBy;
  String createdName;
  DateTime? createdOn;
  String? modifiedBy;
  String? modifiedName;
  DateTime? modifiedOn;
  double total;
  String? notes;
  
  List<OrderDetail> orderDetails;
  
  Order({
    required this.orderKey,
    required this.orderId,
    required this.orderDate,
    required this.guestName,
    this.reservationKey,
    required this.orderStatus,
    required this.recordStatus,
    required this.createdBy,
    required this.createdName,
    this.createdOn,
    this.modifiedBy,
    this.modifiedName,
    this.modifiedOn,
    required this.total,
    this.notes,
    this.orderDetails = const [],
  });
  
  factory Order.fromJson(Map<String, dynamic> json) {
    return Order(
      orderKey: json['orderKey'],
      orderId: json['orderId'],
      orderDate: DateTime.parse(json['orderDate']),
      guestName: json['guestName'],
      reservationKey: json['reservationKey'],
      orderStatus: json['orderStatus'],
      recordStatus: json['recordStatus'],
      createdBy: json['createdBy'],
      createdName: json['createdName'],
      createdOn: json['createdOn'] != null
          ? DateTime.parse(json['createdOn'])
          : null,
      modifiedBy: json['modifiedBy'],
      modifiedName: json['modifiedName'],
      modifiedOn: json['modifiedOn'] != null
          ? DateTime.parse(json['modifiedOn'])
          : null,
      total: json['total'].toDouble(),
      notes: json['notes'],
      orderDetails: (json['orderDetails'] as List?)
          ?.map((e) => OrderDetail.fromJson(e))
          .toList() ?? [],
    );
  }
}
```

---

## 5. Health Profile

### HealthProfile

**Table**: `HealthProfile`

```dart
class HealthProfile {
  String id;
  String userId;
  
  // Basic Info
  int age;
  String? gender;
  String? fullName;
  DateTime? dateOfBirth;
  String? bloodType;          // "A+", "O-", etc.
  
  // Emergency Contact
  String? emergencyContactName;
  String? emergencyContactPhone;
  
  // Chronic Conditions
  bool hasDiabetes;
  bool hasHypertension;
  bool hasAsthma;
  bool hasHeartDisease;
  String? otherDiseases;
  
  // Allergies
  String? drugAllergies;
  String? foodAllergies;
  bool hasLatexAllergy;
  
  // Medications
  String? currentMedicationsJson;  // JSON array
  
  // Insurance
  String? insuranceNumber;
  String? insuranceProvider;
  
  // Emergency
  String? emergencyNotes;
  
  // Physical Measurements
  double? weight;               // kg
  double? height;               // cm
  String? activityLevel;        // "Sedentary", "Light", "Moderate", "Active", "Very Active"
  
  // Timestamps
  DateTime? createdAt;
  DateTime? updatedAt;
  
  HealthProfile({
    required this.id,
    required this.userId,
    required this.age,
    this.gender,
    this.fullName,
    this.dateOfBirth,
    this.bloodType,
    this.emergencyContactName,
    this.emergencyContactPhone,
    this.hasDiabetes = false,
    this.hasHypertension = false,
    this.hasAsthma = false,
    this.hasHeartDisease = false,
    this.otherDiseases,
    this.drugAllergies,
    this.foodAllergies,
    this.hasLatexAllergy = false,
    this.currentMedicationsJson,
    this.insuranceNumber,
    this.insuranceProvider,
    this.emergencyNotes,
    this.weight,
    this.height,
    this.activityLevel,
    this.createdAt,
    this.updatedAt,
  });
  
  factory HealthProfile.fromJson(Map<String, dynamic> json) {
    return HealthProfile(
      id: json['id'],
      userId: json['userId'],
      age: json['age'],
      gender: json['gender'],
      fullName: json['fullName'],
      dateOfBirth: json['dateOfBirth'] != null
          ? DateTime.parse(json['dateOfBirth'])
          : null,
      bloodType: json['bloodType'],
      emergencyContactName: json['emergencyContactName'],
      emergencyContactPhone: json['emergencyContactPhone'],
      hasDiabetes: json['hasDiabetes'] ?? false,
      hasHypertension: json['hasHypertension'] ?? false,
      hasAsthma: json['hasAsthma'] ?? false,
      hasHeartDisease: json['hasHeartDisease'] ?? false,
      otherDiseases: json['otherDiseases'],
      drugAllergies: json['drugAllergies'],
      foodAllergies: json['foodAllergies'],
      hasLatexAllergy: json['hasLatexAllergy'] ?? false,
      currentMedicationsJson: json['currentMedicationsJson'],
      insuranceNumber: json['insuranceNumber'],
      insuranceProvider: json['insuranceProvider'],
      emergencyNotes: json['emergencyNotes'],
      weight: json['weight']?.toDouble(),
      height: json['height']?.toDouble(),
      activityLevel: json['activityLevel'],
      createdAt: json['createdAt'] != null
          ? DateTime.parse(json['createdAt'])
          : null,
      updatedAt: json['updatedAt'] != null
          ? DateTime.parse(json['updatedAt'])
          : null,
    );
  }
  
  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'userId': userId,
      'age': age,
      'gender': gender,
      'fullName': fullName,
      'dateOfBirth': dateOfBirth?.toIso8601String(),
      'bloodType': bloodType,
      'emergencyContactName': emergencyContactName,
      'emergencyContactPhone': emergencyContactPhone,
      'hasDiabetes': hasDiabetes,
      'hasHypertension': hasHypertension,
      'hasAsthma': hasAsthma,
      'hasHeartDisease': hasHeartDisease,
      'otherDiseases': otherDiseases,
      'drugAllergies': drugAllergies,
      'foodAllergies': foodAllergies,
      'hasLatexAllergy': hasLatexAllergy,
      'currentMedicationsJson': currentMedicationsJson,
      'insuranceNumber': insuranceNumber,
      'insuranceProvider': insuranceProvider,
      'emergencyNotes': emergencyNotes,
      'weight': weight,
      'height': height,
      'activityLevel': activityLevel,
      'createdAt': createdAt?.toIso8601String(),
      'updatedAt': updatedAt?.toIso8601String(),
    };
  }
  
  // Helper: Parse medications JSON
  List<Medication> get medications {
    if (currentMedicationsJson == null) return [];
    try {
      final List<dynamic> json = jsonDecode(currentMedicationsJson!);
      return json.map((e) => Medication.fromJson(e)).toList();
    } catch (e) {
      return [];
    }
  }
}

class Medication {
  String name;
  String? dosage;
  String? frequency;
  
  Medication({required this.name, this.dosage, this.frequency});
  
  factory Medication.fromJson(Map<String, dynamic> json) {
    return Medication(
      name: json['name'],
      dosage: json['dosage'],
      frequency: json['frequency'],
    );
  }
  
  Map<String, dynamic> toJson() {
    return {
      'name': name,
      'dosage': dosage,
      'frequency': frequency,
    };
  }
}
```

---

## 6. Traditional Medicine

### BaiThuoc

**Table**: `BaiThuoc`

```dart
class BaiThuoc {
  String id;
  String ten;                 // Recipe name
  String? moTa;               // Description
  String? huongDanSuDung;     // Usage instructions
  String? nguoiDungId;
  DateTime ngayTao;
  String? image;
  int? soLuotThich;
  int? soLuotXem;
  int? trangThai;             // 0: Draft, 1: Published
  
  ApplicationUser? nguoiDung;
  
  BaiThuoc({
    required this.id,
    required this.ten,
    this.moTa,
    this.huongDanSuDung,
    this.nguoiDungId,
    required this.ngayTao,
    this.image,
    this.soLuotThich,
    this.soLuotXem,
    this.trangThai,
    this.nguoiDung,
  });
  
  factory BaiThuoc.fromJson(Map<String, dynamic> json) {
    return BaiThuoc(
      id: json['id'],
      ten: json['ten'],
      moTa: json['moTa'],
      huongDanSuDung: json['huongDanSuDung'],
      nguoiDungId: json['nguoiDungId'],
      ngayTao: DateTime.parse(json['ngayTao']),
      image: json['image'],
      soLuotThich: json['soLuotThich'],
      soLuotXem: json['soLuotXem'],
      trangThai: json['trangThai'],
    );
  }
  
  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'ten': ten,
      'moTa': moTa,
      'huongDanSuDung': huongDanSuDung,
      'nguoiDungId': nguoiDungId,
      'ngayTao': ngayTao.toIso8601String(),
      'image': image,
      'soLuotThich': soLuotThich,
      'soLuotXem': soLuotXem,
      'trangThai': trangThai,
    };
  }
}
```

---

## 7. Relationships

### Friendship

**Table**: `Friendships`

```dart
class Friendship {
  String id;
  String userAId;
  String userBId;
  String status;              // "Pending", "Accepted", "Blocked"
  DateTime createdAt;
  DateTime? acceptedAt;
  
  ApplicationUser? userA;
  ApplicationUser? userB;
  
  Friendship({
    required this.id,
    required this.userAId,
    required this.userBId,
    required this.status,
    required this.createdAt,
    this.acceptedAt,
    this.userA,
    this.userB,
  });
  
  factory Friendship.fromJson(Map<String, dynamic> json) {
    return Friendship(
      id: json['id'],
      userAId: json['userAId'],
      userBId: json['userBId'],
      status: json['status'],
      createdAt: DateTime.parse(json['createdAt']),
      acceptedAt: json['acceptedAt'] != null
          ? DateTime.parse(json['acceptedAt'])
          : null,
    );
  }
}
```

### FriendRequest

```dart
class FriendRequest {
  String id;
  String senderId;
  String receiverId;
  String status;              // "Pending", "Accepted", "Rejected"
  DateTime sentAt;
  DateTime? respondedAt;
  
  ApplicationUser? sender;
  ApplicationUser? receiver;
  
  FriendRequest({
    required this.id,
    required this.senderId,
    required this.receiverId,
    required this.status,
    required this.sentAt,
    this.respondedAt,
    this.sender,
    this.receiver,
  });
  
  factory FriendRequest.fromJson(Map<String, dynamic> json) {
    return FriendRequest(
      id: json['id'],
      senderId: json['senderId'],
      receiverId: json['receiverId'],
      status: json['status'],
      sentAt: DateTime.parse(json['sentAt']),
      respondedAt: json['respondedAt'] != null
          ? DateTime.parse(json['respondedAt'])
          : null,
    );
  }
}
```

---

## 🎯 Usage Examples

### Parsing API Response

```dart
// Single item
final response = await http.get(uri);
if (response.statusCode == 200) {
  final json = jsonDecode(response.body);
  final feedItem = FeedItemDto.fromJson(json);
}

// List of items
final response = await http.get(uri);
if (response.statusCode == 200) {
  final List<dynamic> jsonList = jsonDecode(response.body);
  final items = jsonList
      .map((json) => FeedItemDto.fromJson(json))
      .toList();
}

// Nested object
final response = await http.get(uri);
if (response.statusCode == 200) {
  final json = jsonDecode(response.body);
  final cart = GioHang.fromJson(json);
  // cart.chiTiets is automatically parsed
}
```

### Sending to API

```dart
// JSON body
final response = await http.post(
  uri,
  headers: {'Content-Type': 'application/json'},
  body: jsonEncode(healthProfile.toJson()),
);

// Form data
var request = http.MultipartRequest('POST', uri);
request.fields['ten'] = baiThuoc.ten;
request.fields['moTa'] = baiThuoc.moTa ?? '';
if (imageFile != null) {
  request.files.add(
    await http.MultipartFile.fromPath('Image', imageFile.path),
  );
}
var response = await request.send();
```

---

## 📝 Notes

### Naming Conventions
- **Backend (C#)**: PascalCase for properties
- **API Response**: camelCase (configured in `Program.cs`)
- **Flutter**: camelCase for properties

### Null Safety
- All optional fields use `?` in Dart
- Check for null before accessing nested properties

### Date Handling
```dart
// Parse from API
DateTime.parse(json['ngayDang'])

// Format for API
dateTime.toIso8601String()

// Display
DateFormat('dd/MM/yyyy HH:mm').format(dateTime)
```

### Guid/UUID
```dart
// Generate new
import 'package:uuid/uuid.dart';
final uuid = Uuid();
final id = uuid.v4();

// Parse from string
final guid = json['id']; // Already a string
```

---

**Last Updated**: November 8, 2025
