# Đặc tả API - Quản lý Video và Bài viết

> Tài liệu mô tả chi tiết các API endpoints được sử dụng trong giao diện quản lý Video và Bài viết.

---

## Mục lục

1. [Video APIs](#1-video-apis)
2. [Post APIs](#2-post-apis)
3. [Common APIs](#3-common-apis)
4. [Data Models](#4-data-models)

---

## 1. Video APIs

### 1.1. Lấy danh sách Video

```http
GET /api/videos
```

**Query Parameters:**

| Param | Type | Description |
|-------|------|-------------|
| `page` | int | Trang hiện tại (default: 1) |
| `pageSize` | int | Số item/trang (default: 10) |
| `search` | string | Tìm kiếm theo title |
| `type` | string | `short` \| `expert` \| null (tất cả) |
| `isPremium` | bool? | Lọc theo premium |
| `status` | string | `draft` \| `published` \| `hidden` |
| `categoryId` | int? | Lọc theo danh mục |
| `expertId` | int? | Lọc theo chuyên gia |

**Response:**

```json
{
  "data": [
    {
      "videoId": 1,
      "title": "Kỹ thuật thở giảm stress",
      "description": "...",
      "thumbnailUrl": "https://...",
      "videoUrl": "https://...",
      "durationSeconds": 125,
      "isShort": true,
      "isPremium": true,
      "status": "published",
      "publishedAt": "2025-01-12T00:00:00",
      "createdAt": "2025-01-10T00:00:00",
      "expert": {
        "expertId": 1,
        "name": "ThS. Nguyễn Mai Linh"
      },
      "categories": [
        { "categoryId": 1, "name": "Quản lý stress" }
      ],
      "tags": [
        { "tagId": 1, "name": "stress" },
        { "tagId": 2, "name": "tamly" }
      ],
      "stats": {
        "viewCount": 2300,
        "likeCount": 234
      }
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 128,
    "totalPages": 13
  }
}
```

---

### 1.2. Lấy chi tiết Video

```http
GET /api/videos/{videoId}
```

**Response:** Trả về object video đầy đủ như trên.

---

### 1.3. Tạo Video mới

```http
POST /api/videos
Content-Type: application/json
```

**Request Body:**

```json
{
  "expertId": 1,
  "title": "Tiêu đề video",
  "description": "Mô tả video",
  "thumbnailUrl": "https://...",
  "videoUrl": "https://...",
  "durationSeconds": 125,
  "isShort": true,
  "isPremium": false,
  "status": "draft",
  "publishedAt": null,
  "categoryIds": [1, 2],
  "tagIds": [1, 3, 5]
}
```

**Response:** `201 Created` với object video vừa tạo.

---

### 1.4. Cập nhật Video

```http
PUT /api/videos/{videoId}
Content-Type: application/json
```

**Request Body:** Tương tự POST, chỉ gửi các field cần update.

**Response:** `200 OK` với object video đã cập nhật.

---

### 1.5. Xóa Video

```http
DELETE /api/videos/{videoId}
```

**Response:** `204 No Content`

> Xóa CASCADE: VideoCategories, VideoTags, VideoStats, VideoLikes, VideoViews

---

### 1.6. Thống kê Video (Dashboard Stats)

```http
GET /api/videos/stats
```

**Query Parameters:**

| Param | Type | Description |
|-------|------|-------------|
| `fromDate` | datetime | Ngày bắt đầu (7 ngày gần nhất mặc định) |
| `toDate` | datetime | Ngày kết thúc |

**Response:**

```json
{
  "totalVideos": 128,
  "premiumVideos": 42,
  "premiumPercent": 32.8,
  "totalViews": 1200000,
  "avgLikesPerVideo": 356,
  "newVideosThisWeek": 6
}
```

---

## 2. Post APIs

### 2.1. Lấy danh sách Bài viết

```http
GET /api/posts
```

**Query Parameters:**

| Param | Type | Description |
|-------|------|-------------|
| `page` | int | Trang hiện tại (default: 1) |
| `pageSize` | int | Số item/trang (default: 10) |
| `search` | string | Tìm kiếm theo title |
| `isPremium` | bool? | Lọc theo premium |
| `status` | string | `draft` \| `published` \| `hidden` |
| `categoryId` | int? | Lọc theo danh mục |
| `expertId` | int? | Lọc theo tác giả/chuyên gia |

**Response:**

```json
{
  "data": [
    {
      "postId": 1,
      "title": "Cách quản lý stress hiệu quả",
      "summary": "Tóm tắt bài viết...",
      "content": "<p>Nội dung HTML...</p>",
      "thumbnailUrl": "https://...",
      "isPremium": true,
      "status": "published",
      "publishedAt": "2025-01-10T00:00:00",
      "createdAt": "2025-01-08T00:00:00",
      "expert": {
        "expertId": 1,
        "name": "ThS. Nguyễn Mai Linh"
      },
      "categories": [
        { "categoryId": 1, "name": "Tâm lý học" }
      ],
      "tags": [
        { "tagId": 1, "name": "stress" },
        { "tagId": 2, "name": "congviec" }
      ],
      "stats": {
        "viewCount": 15200,
        "likeCount": 1200
      }
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 256,
    "totalPages": 26
  }
}
```

---

### 2.2. Lấy chi tiết Bài viết

```http
GET /api/posts/{postId}
```

**Response:** Object post đầy đủ.

---

### 2.3. Tạo Bài viết mới

```http
POST /api/posts
Content-Type: application/json
```

**Request Body:**

```json
{
  "expertId": 1,
  "title": "Tiêu đề bài viết",
  "summary": "Mô tả ngắn",
  "content": "<p>Nội dung HTML/Markdown</p>",
  "thumbnailUrl": "https://...",
  "isPremium": false,
  "status": "draft",
  "publishedAt": null,
  "categoryIds": [1, 2],
  "tagIds": [1, 3]
}
```

**Response:** `201 Created`

---

### 2.4. Cập nhật Bài viết

```http
PUT /api/posts/{postId}
Content-Type: application/json
```

**Request Body:** Tương tự POST.

**Response:** `200 OK`

---

### 2.5. Xóa Bài viết

```http
DELETE /api/posts/{postId}
```

**Response:** `204 No Content`

> Xóa CASCADE: PostCategories, PostTags, PostStats, PostLikes, PostViews

---

### 2.6. Thống kê Bài viết (Dashboard Stats)

```http
GET /api/posts/stats
```

**Response:**

```json
{
  "totalPosts": 256,
  "premiumPosts": 85,
  "premiumPercent": 33.2,
  "totalViews": 2500000,
  "avgLikesPerPost": 128,
  "newPostsThisWeek": 12
}
```

---

## 3. Common APIs

### 3.1. Danh mục (Categories)

```http
GET /api/categories
```

**Response:**

```json
[
  {
    "categoryId": 1,
    "name": "Quản lý stress",
    "slug": "quan-ly-stress",
    "description": "...",
    "isActive": true
  }
]
```

---

### 3.2. Tags

```http
GET /api/tags
```

**Query Parameters:**

| Param | Type | Description |
|-------|------|-------------|
| `search` | string | Tìm kiếm tag theo tên |

**Response:**

```json
[
  { "tagId": 1, "name": "stress", "slug": "stress" },
  { "tagId": 2, "name": "tamly", "slug": "tamly" }
]
```

---

### 3.3. Tạo Tag mới

```http
POST /api/tags
```

**Request Body:**

```json
{
  "name": "meditation"
}
```

**Response:** `201 Created` với tag vừa tạo (slug tự generate).

---

### 3.4. Chuyên gia (Experts)

```http
GET /api/experts
```

**Response:**

```json
[
  {
    "expertId": 1,
    "name": "ThS. Nguyễn Mai Linh",
    "specialty": "Tâm lý học",
    "avatarUrl": "https://..."
  }
]
```

---

## 4. Data Models

### 4.1. Video Entity

| Column | Type | Description |
|--------|------|-------------|
| `video_id` | INT (PK) | ID tự tăng |
| `expert_id` | INT (FK) | Chuyên gia sở hữu |
| `title` | NVARCHAR(255) | Tiêu đề |
| `description` | NVARCHAR(MAX) | Mô tả |
| `thumbnail_url` | NVARCHAR(500) | URL ảnh thumbnail |
| `video_url` | NVARCHAR(500) | URL video |
| `duration_seconds` | INT | Thời lượng (giây) |
| `is_short` | BIT | Video ngắn (TikTok-style) |
| `is_premium` | BIT | Yêu cầu VIP |
| `status` | NVARCHAR(20) | draft \| published \| hidden |
| `published_at` | DATETIME2 | Ngày publish |
| `created_at` | DATETIME2 | Ngày tạo |
| `updated_at` | DATETIME2 | Ngày cập nhật |

---

### 4.2. Post Entity

| Column | Type | Description |
|--------|------|-------------|
| `post_id` | INT (PK) | ID tự tăng |
| `expert_id` | INT (FK) | Tác giả |
| `title` | NVARCHAR(255) | Tiêu đề |
| `summary` | NVARCHAR(500) | Tóm tắt |
| `content` | NVARCHAR(MAX) | Nội dung HTML/Markdown |
| `thumbnail_url` | NVARCHAR(500) | URL ảnh thumbnail |
| `is_premium` | BIT | Yêu cầu VIP |
| `status` | NVARCHAR(20) | draft \| published \| hidden |
| `published_at` | DATETIME2 | Ngày publish |
| `created_at` | DATETIME2 | Ngày tạo |
| `updated_at` | DATETIME2 | Ngày cập nhật |

---

### 4.3. Relationship Tables

| Table | Description |
|-------|-------------|
| `VideoCategories` | Many-to-Many Video ↔ Category |
| `VideoTags` | Many-to-Many Video ↔ Tag |
| `VideoStats` | 1-1 Thống kê view/like |
| `VideoLikes` | Many-to-Many User ↔ Video (like) |
| `VideoViews` | Log lượt xem (có thể guest) |
| `PostCategories` | Many-to-Many Post ↔ Category |
| `PostTags` | Many-to-Many Post ↔ Tag |
| `PostStats` | 1-1 Thống kê view/like |
| `PostLikes` | Many-to-Many User ↔ Post (like) |
| `PostViews` | Log lượt xem |

---

## HTTP Status Codes

| Code | Meaning |
|------|---------|
| `200` | OK - Thành công |
| `201` | Created - Tạo mới thành công |
| `204` | No Content - Xóa thành công |
| `400` | Bad Request - Dữ liệu không hợp lệ |
| `401` | Unauthorized - Chưa đăng nhập |
| `403` | Forbidden - Không có quyền |
| `404` | Not Found - Không tìm thấy |
| `500` | Internal Server Error |

---

## Ghi chú Implementation

1. **Pagination**: Sử dụng offset-based pagination với `page` và `pageSize`.

2. **Search**: Full-text search trên `title` và `description`.

3. **Stats Update**: 
   - `VideoStats` / `PostStats` được update qua background job hoặc trigger.
   - Có thể cache stats để giảm tải.

4. **File Upload**: 
   - Thumbnail và Video URL nên được upload trước qua API riêng.
   - Recommend: Azure Blob Storage hoặc Cloudinary.

5. **Soft Delete**: Có thể thêm `is_deleted` flag thay vì xóa cứng.
