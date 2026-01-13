using System;
using System.Collections.Generic;

namespace AdminPortal.ViewModels
{
    // Post response DTO
    public class PostDto
    {
        public int PostId { get; set; }
        public string Title { get; set; } = "";
        public string? Summary { get; set; }
        public string Content { get; set; } = "";
        public string? ThumbnailUrl { get; set; }
        public bool IsPremium { get; set; }
        public string Status { get; set; } = "draft";
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ExpertSimpleDto? Expert { get; set; }
        public List<CategorySimpleDto> Categories { get; set; } = new();
        public List<TagSimpleDto> Tags { get; set; } = new();
        public ContentStatsDto? Stats { get; set; }
    }

    // Post create request DTO
    public class PostCreateDto
    {
        public int? ExpertId { get; set; }
        public string Title { get; set; } = "";
        public string? Summary { get; set; }
        public string Content { get; set; } = "";
        public string? ThumbnailUrl { get; set; }
        public bool IsPremium { get; set; }
        public string Status { get; set; } = "draft";
        public DateTime? PublishedAt { get; set; }
        public List<int> CategoryIds { get; set; } = new();
        public List<int> TagIds { get; set; } = new();
    }

    // Post update request DTO
    public class PostUpdateDto
    {
        public int? ExpertId { get; set; }
        public string? Title { get; set; }
        public string? Summary { get; set; }
        public string? Content { get; set; }
        public string? ThumbnailUrl { get; set; }
        public bool? IsPremium { get; set; }
        public string? Status { get; set; }
        public DateTime? PublishedAt { get; set; }
        public List<int>? CategoryIds { get; set; }
        public List<int>? TagIds { get; set; }
    }
}
