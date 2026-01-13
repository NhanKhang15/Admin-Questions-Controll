using System;
using System.Collections.Generic;

namespace AdminPortal.ViewModels
{
    // Video response DTO
    public class VideoDto
    {
        public int VideoId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string VideoUrl { get; set; } = "";
        public int DurationSeconds { get; set; }
        public bool IsShort { get; set; }
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

    // Video create/update request DTO
    public class VideoCreateDto
    {
        public int? ExpertId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string VideoUrl { get; set; } = "";
        public int DurationSeconds { get; set; }
        public bool IsShort { get; set; }
        public bool IsPremium { get; set; }
        public string Status { get; set; } = "draft";
        public DateTime? PublishedAt { get; set; }
        public List<int> CategoryIds { get; set; } = new();
        public List<int> TagIds { get; set; } = new();
    }

    public class VideoUpdateDto
    {
        public int? ExpertId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? VideoUrl { get; set; }
        public int? DurationSeconds { get; set; }
        public bool? IsShort { get; set; }
        public bool? IsPremium { get; set; }
        public string? Status { get; set; }
        public DateTime? PublishedAt { get; set; }
        public List<int>? CategoryIds { get; set; }
        public List<int>? TagIds { get; set; }
    }
}
