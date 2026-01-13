using System;
using System.Collections.Generic;

namespace AdminPortal.ViewModels
{
    // Generic paginated result wrapper
    public class PaginatedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
    }

    public class PaginationInfo
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    // Stats DTOs
    public class VideoStatsDto
    {
        public int TotalVideos { get; set; }
        public int PremiumVideos { get; set; }
        public double PremiumPercent { get; set; }
        public long TotalViews { get; set; }
        public double AvgLikesPerVideo { get; set; }
        public int NewVideosThisWeek { get; set; }
    }

    public class PostStatsDto
    {
        public int TotalPosts { get; set; }
        public int PremiumPosts { get; set; }
        public double PremiumPercent { get; set; }
        public long TotalViews { get; set; }
        public double AvgLikesPerPost { get; set; }
        public int NewPostsThisWeek { get; set; }
    }

    // Lightweight DTOs for nested objects
    public class ExpertSimpleDto
    {
        public int ExpertId { get; set; }
        public string Name { get; set; } = "";
        public string? AvatarUrl { get; set; }
    }

    public class CategorySimpleDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
    }

    public class TagSimpleDto
    {
        public int TagId { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
    }

    public class ContentStatsDto
    {
        public long ViewCount { get; set; }
        public long LikeCount { get; set; }
    }
}
