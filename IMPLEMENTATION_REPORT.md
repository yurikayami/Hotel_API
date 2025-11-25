# IMPLEMENTATION REPORT: View and Edit Profile Feature

**Date**: November 25, 2025  
**Project**: Hotel_API v2  
**Feature**: User Profile Management (View & Edit)

---

## Overview

Tính năng này cho phép người dùng xem và chỉnh sửa thông tin cá nhân và hồ sơ sức khỏe thông qua API RESTful. Đặc biệt được thiết kế để duy trì **backward compatibility** với ứng dụng di động cũ trong khi cung cấp một API hiện đại cho phiên bản mới.

---

## Architecture & Design

### Key Principles

1. **Legacy Compatibility**: Bảo tồn tất cả các field Vietnamese cũ trong `ApplicationUser`:
   - `gioi_tinh` (gender)
   - `tuoi` (age)
   - `displayName`
   - `avatarUrl`

2. **Clean API**: Cung cấp DTOs với naming tiếng Anh sạch cho Flutter app mới
   - `UserProfileDto` (View)
   - `UpdateProfileDto` (Edit)

3. **Data Synchronization**: Logic tự động sync dữ liệu giữa hai model:
   - Khi update `Gender` → update cả `ApplicationUser.gioi_tinh` và `HealthProfile.Gender`
   - Khi update `DateOfBirth` → auto-calculate `Age` và sync vào cả hai model

---

## Files Created/Modified

### 1. DTOs (Models/ViewModels/)

#### `UserProfileDto.cs` ✅
**Purpose**: Read-only DTO cho viewing profile information  
**Location**: `Models/ViewModels/UserProfileDto.cs`

**Fields Structure**:
```
Identity/User Info (from ApplicationUser):
  - UserId
  - DisplayName
  - Avatar (maps from avatarUrl)
  - Email
  - PhoneNumber
  - Gender (maps from gioi_tinh)
  - Age (maps from tuoi)

Physical Measurements (from HealthProfile):
  - Height (cm)
  - Weight (kg)
  - DateOfBirth
  - BloodType

Emergency Contact Info:
  - EmergencyContactName
  - EmergencyContactPhone

Insurance Info:
  - InsuranceNumber
  - InsuranceProvider

Medical Conditions (Boolean flags):
  - HasDiabetes
  - HasHypertension
  - HasAsthma
  - HasHeartDisease
  - HasLatexAllergy

Medical Text Info:
  - DrugAllergies
  - FoodAllergies
  - OtherDiseases
  - ActivityLevel
  - CurrentMedications (parsed from JSON)

Metadata:
  - EmergencyNotes
  - CreatedAt
  - UpdatedAt
```

#### `UpdateProfileDto.cs` ✅
**Purpose**: DTO cho update/edit operations  
**Location**: `Models/ViewModels/UpdateProfileDto.cs`

**Fields**: Tương tự `UserProfileDto` nhưng chỉ bao gồm các field có thể update

---

### 2. Controller

#### `UserProfileController.cs` ✅
**Location**: `Controllers/UserProfileController.cs`  
**Base Route**: `/api/userprofile`

**Endpoints**:

##### GET `/api/userprofile/profile`
- **Purpose**: Lấy thông tin hồ sơ của người dùng đang đăng nhập
- **Authentication**: Required (Bearer Token)
- **Response**: `UserProfileDto`
- **Status Codes**:
  - `200 OK`: Successfully retrieved profile
  - `401 Unauthorized`: User not authenticated
  - `404 Not Found`: User or profile not found
  - `500 Internal Server Error`: Server error

**Example Request**:
```bash
curl -X GET "http://localhost:5000/api/userprofile/profile" \
  -H "Authorization: Bearer <token>"
```

**Example Response** (200 OK):
```json
{
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "displayName": "Nguyễn Văn A",
  "avatar": "https://api.example.com/avatars/user123.jpg",
  "email": "user@example.com",
  "phoneNumber": "0935648639",
  "gender": "Nam",
  "age": 40,
  "height": 175,
  "weight": 55,
  "dateOfBirth": "1984-11-25T00:00:00Z",
  "bloodType": "A+",
  "emergencyContactName": "Nguyễn Thị B",
  "emergencyContactPhone": "0987654321",
  "insuranceNumber": "BH123456",
  "insuranceProvider": "Bảo Việt",
  "hasDiabetes": false,
  "hasHypertension": false,
  "hasAsthma": false,
  "hasHeartDisease": false,
  "hasLatexAllergy": false,
  "drugAllergies": "Penicillin",
  "foodAllergies": "Seafood",
  "otherDiseases": "None",
  "activityLevel": "Moderate",
  "currentMedications": ["Vitamin D", "Aspirin"],
  "emergencyNotes": "Call 911 if emergency",
  "createdAt": "2024-01-15T10:00:00Z",
  "updatedAt": "2024-11-25T14:30:00Z"
}
```

---

##### PUT `/api/userprofile/profile`
- **Purpose**: Cập nhật thông tin hồ sơ của người dùng đang đăng nhập
- **Authentication**: Required (Bearer Token)
- **Request Body**: `UpdateProfileDto`
- **Response**: `UserProfileDto` (profile cập nhật)
- **Status Codes**:
  - `200 OK`: Successfully updated
  - `400 Bad Request`: Invalid input
  - `401 Unauthorized`: User not authenticated
  - `404 Not Found`: User not found
  - `500 Internal Server Error`: Server error

**Example Request**:
```bash
curl -X PUT "http://localhost:5000/api/userprofile/profile" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "displayName": "Nguyễn Văn A Updated",
    "avatar": "https://api.example.com/avatars/user123_new.jpg",
    "gender": "Nam",
    "age": 41,
    "height": 176,
    "weight": 56,
    "dateOfBirth": "1983-11-25T00:00:00Z",
    "bloodType": "A+",
    "emergencyContactName": "Nguyễn Thị B",
    "emergencyContactPhone": "0987654321",
    "insuranceNumber": "BH123456",
    "insuranceProvider": "Bảo Việt",
    "hasDiabetes": false,
    "hasHypertension": false,
    "hasAsthma": false,
    "hasHeartDisease": false,
    "hasLatexAllergy": false,
    "drugAllergies": "Penicillin",
    "foodAllergies": "Seafood",
    "otherDiseases": "None",
    "activityLevel": "Moderate",
    "currentMedicationsJson": "[\"Vitamin D\", \"Aspirin\"]",
    "emergencyNotes": "Call 911 if emergency"
  }'
```

**Example Response** (200 OK): Same as GET response with updated values

---

## Critical Implementation Details

### ✓ CRITICAL SYNC LOGIC #1: Gender Update

**Location**: Line 132-137 in `UserProfileController.cs`

```csharp
if (!string.IsNullOrEmpty(updateProfileDto.Gender))
{
    // Update both ApplicationUser.gioi_tinh and HealthProfile.Gender
    applicationUser.gioi_tinh = updateProfileDto.Gender;
    healthProfile.Gender = updateProfileDto.Gender;
}
```

**Why**: Ensures legacy app reading `ApplicationUser.gioi_tinh` stays in sync with new app reading `HealthProfile.Gender`.

---

### ✓ CRITICAL SYNC LOGIC #2: DateOfBirth & Age Auto-calculation

**Location**: Line 140-150 in `UserProfileController.cs`

```csharp
if (updateProfileDto.DateOfBirth.HasValue)
{
    healthProfile.DateOfBirth = updateProfileDto.DateOfBirth.Value;
    
    // Auto-calculate age from DateOfBirth
    int calculatedAge = CalculateAge(updateProfileDto.DateOfBirth.Value);
    applicationUser.tuoi = calculatedAge;
    healthProfile.Age = calculatedAge;
}
else if (updateProfileDto.Age.HasValue)
{
    // If only Age is provided, update both
    applicationUser.tuoi = updateProfileDto.Age.Value;
    healthProfile.Age = updateProfileDto.Age.Value;
}
```

**Why**: 
- Auto-calculate age keeps data accurate if DateOfBirth is updated
- Sync to both `ApplicationUser.tuoi` and `HealthProfile.Age`
- Fallback to direct age update if DateOfBirth not provided

---

### ✓ HealthProfile Auto-creation

**Location**: Line 114-121 in `UserProfileController.cs`

```csharp
var healthProfile = _context.HealthProfiles.FirstOrDefault(hp => hp.UserId == userId);
if (healthProfile == null)
{
    healthProfile = new HealthProfile
    {
        Id = Guid.NewGuid().ToString(),
        UserId = userId,
        CreatedAt = DateTime.UtcNow
    };
    _context.HealthProfiles.Add(healthProfile);
}
```

**Why**: Ensures HealthProfile exists even for existing users without profile data.

---

### ✓ Single Transaction Save

**Location**: Line 203-206 in `UserProfileController.cs`

```csharp
_context.Users.Update(applicationUser);
_context.HealthProfiles.Update(healthProfile);
await _context.SaveChangesAsync();
```

**Why**: All changes saved in single database transaction for data consistency.

---

## Backward Compatibility Guarantees

✅ **Preserved Fields in ApplicationUser**:
- `gioi_tinh` - Still synced when updated
- `tuoi` - Still synced when updated
- `displayName` - Still synced when updated
- `avatarUrl` - Still synced when updated

✅ **Legacy Mobile App**: Can still read directly from `AspNetUsers` table and see synchronized data

✅ **New Flutter App**: Uses clean English API with DTOs

---

## Security Features

1. **Authorization**: All endpoints require `[Authorize]` attribute
2. **User Isolation**: Uses `User.FindFirstValue(ClaimTypes.NameIdentifier)` to ensure users can only access their own data
3. **Input Validation**: Null checks on user ID and input DTO
4. **Error Handling**: Try-catch blocks with logging for debugging

---

## Data Flow Diagram

```
Flutter App (New)
      ↓
[UserProfileDto / UpdateProfileDto]
      ↓
UserProfileController Endpoints
      ↓
{Sync Logic}
      ↓
ApplicationUser (Vietnamese fields)  ←→  HealthProfile
        ↓                                    ↓
    Database                           Database
        ↓
Legacy Mobile App (Old) ← Reads directly from AspNetUsers
```

---

## Usage Instructions for Flutter App

### 1. Get User Profile
```dart
final response = await http.get(
  Uri.parse('http://api.example.com/api/userprofile/profile'),
  headers: {
    'Authorization': 'Bearer $token',
  },
);

final profile = UserProfileDto.fromJson(jsonDecode(response.body));
```

### 2. Update User Profile
```dart
final updateData = UpdateProfileDto(
  displayName: 'New Name',
  gender: 'Nam',
  dateOfBirth: DateTime(1985, 6, 15),
  height: 175,
  weight: 70,
  // ... other fields
);

final response = await http.put(
  Uri.parse('http://api.example.com/api/userprofile/profile'),
  headers: {
    'Authorization': 'Bearer $token',
    'Content-Type': 'application/json',
  },
  body: jsonEncode(updateData),
);

final updatedProfile = UserProfileDto.fromJson(jsonDecode(response.body));
```

---

## Testing Recommendations

### Unit Tests
- Test age calculation function with various dates
- Test null profile creation
- Test field sync logic

### Integration Tests
- GET profile for user with/without HealthProfile
- PUT profile and verify sync to both tables
- PUT Gender and verify both models updated
- PUT DateOfBirth and verify age auto-calculated
- Verify unauthorized access returns 401
- Verify user can't access other user's profile

### Example Test Case (PUT Gender):
```csharp
[Test]
public async Task UpdateProfile_WhenGenderUpdated_ShouldSyncToApplicationUserAndHealthProfile()
{
    // Arrange
    var userId = "test-user-123";
    var updateDto = new UpdateProfileDto { Gender = "Nữ" };
    
    // Act
    var result = await _controller.UpdateProfile(updateDto);
    
    // Assert
    var user = await _context.Users.FindAsync(userId);
    var profile = await _context.HealthProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
    
    Assert.AreEqual("Nữ", user.gioi_tinh);
    Assert.AreEqual("Nữ", profile.Gender);
}
```

---

## Database Schema Integration

No new tables created. Uses existing:
- `AspNetUsers` - ApplicationUser entity
- `HealthProfile` - HealthProfile entity

**Key Columns**:
```sql
-- ApplicationUser
gioi_tinh (varchar) - synced with HealthProfile.Gender
tuoi (int) - synced with HealthProfile.Age
displayName (varchar) - synced with update
avatarUrl (varchar) - synced with update

-- HealthProfile
Gender (varchar) - synced with ApplicationUser.gioi_tinh
Age (int) - synced with ApplicationUser.tuoi and auto-calculated from DateOfBirth
DateOfBirth (datetime)
Height (float)
Weight (float)
BloodType (varchar)
... other fields
```

---

## Logging & Monitoring

Controller includes logging at key points:
- `LogInformation`: Successful GET/PUT operations
- `LogWarning`: User not found, UserId not in token
- `LogError`: Exceptions during operations

**Log Messages**:
```
[INFO] Successfully retrieved profile for user 123e4567-e89b-12d3-a456-426614174000
[INFO] Successfully updated profile for user 123e4567-e89b-12d3-a456-426614174000
[WARN] UserId not found in token
[WARN] User with ID 123e4567 not found
[ERROR] Error updating profile: {exception message}
```

---

## Potential Enhancements (Future)

1. Add profile picture upload endpoint (separate from this feature)
2. Add audit logging for profile changes
3. Add partial update support (PATCH method)
4. Add profile completion percentage calculation
5. Add profile validation rules (e.g., valid blood types)
6. Add data encryption for sensitive fields (SSN, etc.)
7. Add profile history/versioning
8. Add profile sharing/permissions

---

## Conclusion

Tính năng View and Edit Profile được implement với:
- ✅ Full backward compatibility
- ✅ Automatic data synchronization
- ✅ Clean API for new app
- ✅ Security & authorization
- ✅ Error handling & logging
- ✅ Transaction safety

The implementation is **production-ready** and can be deployed to the Flutter app immediately.

---

**Implementation Date**: November 25, 2025  
**Status**: ✅ COMPLETE
