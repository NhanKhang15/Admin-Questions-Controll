using AdminPortal.Data;
using AdminPortal.Models;
using AdminPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminPortal.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideosController : ControllerBase
    {
        private readonly FloriaDbContext _db;
        public VideosController(FloriaDbContext db) => _db = db;

        // GET /api/videos
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<VideoDto>>> GetVideos(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? type = null,
            [FromQuery] bool? isPremium = null,
            [FromQuery] string? status = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] int? expertId = null)
        {
            var query = _db.Videos
                .Include(v => v.Expert)
                .Include(v => v.VideoCategories).ThenInclude(vc => vc.Category)
                .Include(v => v.VideoTags).ThenInclude(vt => vt.Tag)
                .Include(v => v.Stats)
                .AsQueryable();

            // Filters
            if (!string.IsNullOrEmpty(search))
                query = query.Where(v => v.Title.Contains(search) || (v.Description != null && v.Description.Contains(search)));

            if (type == "short")
                query = query.Where(v => v.IsShort);
            else if (type == "expert")
                query = query.Where(v => !v.IsShort);

            if (isPremium.HasValue)
                query = query.Where(v => v.IsPremium == isPremium.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(v => v.Status == status);

            if (categoryId.HasValue)
                query = query.Where(v => v.VideoCategories.Any(vc => vc.CategoryId == categoryId.Value));

            if (expertId.HasValue)
                query = query.Where(v => v.ExpertId == expertId.Value);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var videos = await query
                .OrderByDescending(v => v.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new VideoDto
                {
                    VideoId = v.VideoId,
                    Title = v.Title,
                    Description = v.Description,
                    ThumbnailUrl = v.ThumbnailUrl,
                    VideoUrl = v.VideoUrl,
                    DurationSeconds = v.DurationSeconds,
                    IsShort = v.IsShort,
                    IsPremium = v.IsPremium,
                    Status = v.Status,
                    PublishedAt = v.PublishedAt,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt,
                    Expert = v.Expert != null ? new ExpertSimpleDto
                    {
                        ExpertId = v.Expert.ExpertId,
                        Name = v.Expert.FullName,
                        AvatarUrl = null
                    } : null,
                    Categories = v.VideoCategories.Select(vc => new CategorySimpleDto
                    {
                        CategoryId = vc.Category.CategoryId,
                        Name = vc.Category.Name,
                        Slug = vc.Category.Slug
                    }).ToList(),
                    Tags = v.VideoTags.Select(vt => new TagSimpleDto
                    {
                        TagId = vt.Tag.TagId,
                        Name = vt.Tag.Name,
                        Slug = vt.Tag.Slug
                    }).ToList(),
                    Stats = v.Stats != null ? new ContentStatsDto
                    {
                        ViewCount = v.Stats.ViewCount,
                        LikeCount = v.Stats.LikeCount
                    } : null
                })
                .ToListAsync();

            return Ok(new PaginatedResult<VideoDto>
            {
                Data = videos,
                Pagination = new PaginationInfo
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                }
            });
        }

        // GET /api/videos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<VideoDto>> GetVideo(int id)
        {
            var v = await _db.Videos
                .Include(x => x.Expert)
                .Include(x => x.VideoCategories).ThenInclude(vc => vc.Category)
                .Include(x => x.VideoTags).ThenInclude(vt => vt.Tag)
                .Include(x => x.Stats)
                .FirstOrDefaultAsync(x => x.VideoId == id);

            if (v == null) return NotFound();

            return Ok(new VideoDto
            {
                VideoId = v.VideoId,
                Title = v.Title,
                Description = v.Description,
                ThumbnailUrl = v.ThumbnailUrl,
                VideoUrl = v.VideoUrl,
                DurationSeconds = v.DurationSeconds,
                IsShort = v.IsShort,
                IsPremium = v.IsPremium,
                Status = v.Status,
                PublishedAt = v.PublishedAt,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt,
                Expert = v.Expert != null ? new ExpertSimpleDto
                {
                    ExpertId = v.Expert.ExpertId,
                    Name = v.Expert.FullName,
                    AvatarUrl = null
                } : null,
                Categories = v.VideoCategories.Select(vc => new CategorySimpleDto
                {
                    CategoryId = vc.Category.CategoryId,
                    Name = vc.Category.Name,
                    Slug = vc.Category.Slug
                }).ToList(),
                Tags = v.VideoTags.Select(vt => new TagSimpleDto
                {
                    TagId = vt.Tag.TagId,
                    Name = vt.Tag.Name,
                    Slug = vt.Tag.Slug
                }).ToList(),
                Stats = v.Stats != null ? new ContentStatsDto
                {
                    ViewCount = v.Stats.ViewCount,
                    LikeCount = v.Stats.LikeCount
                } : null
            });
        }

        // POST /api/videos
        [HttpPost]
        public async Task<ActionResult<VideoDto>> CreateVideo([FromBody] VideoCreateDto dto)
        {
            var video = new Video
            {
                ExpertId = dto.ExpertId,
                Title = dto.Title,
                Description = dto.Description,
                ThumbnailUrl = dto.ThumbnailUrl,
                VideoUrl = dto.VideoUrl,
                DurationSeconds = dto.DurationSeconds,
                IsShort = dto.IsShort,
                IsPremium = dto.IsPremium,
                Status = dto.Status,
                PublishedAt = dto.PublishedAt,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _db.Videos.Add(video);
            await _db.SaveChangesAsync();

            // Add categories
            foreach (var catId in dto.CategoryIds)
            {
                _db.VideoCategories.Add(new VideoCategory { VideoId = video.VideoId, CategoryId = catId });
            }

            // Add tags
            foreach (var tagId in dto.TagIds)
            {
                _db.VideoTags.Add(new VideoTag { VideoId = video.VideoId, TagId = tagId });
            }

            // Create stats record
            _db.VideoStats.Add(new VideoStats { VideoId = video.VideoId });

            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVideo), new { id = video.VideoId }, await GetVideo(video.VideoId));
        }

        // PUT /api/videos/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<VideoDto>> UpdateVideo(int id, [FromBody] VideoUpdateDto dto)
        {
            var video = await _db.Videos
                .Include(v => v.VideoCategories)
                .Include(v => v.VideoTags)
                .FirstOrDefaultAsync(v => v.VideoId == id);

            if (video == null) return NotFound();

            // Update fields if provided
            if (dto.ExpertId.HasValue) video.ExpertId = dto.ExpertId;
            if (dto.Title != null) video.Title = dto.Title;
            if (dto.Description != null) video.Description = dto.Description;
            if (dto.ThumbnailUrl != null) video.ThumbnailUrl = dto.ThumbnailUrl;
            if (dto.VideoUrl != null) video.VideoUrl = dto.VideoUrl;
            if (dto.DurationSeconds.HasValue) video.DurationSeconds = dto.DurationSeconds.Value;
            if (dto.IsShort.HasValue) video.IsShort = dto.IsShort.Value;
            if (dto.IsPremium.HasValue) video.IsPremium = dto.IsPremium.Value;
            if (dto.Status != null) video.Status = dto.Status;
            if (dto.PublishedAt.HasValue) video.PublishedAt = dto.PublishedAt;

            video.UpdatedAt = DateTime.Now;

            // Update categories if provided
            if (dto.CategoryIds != null)
            {
                _db.VideoCategories.RemoveRange(video.VideoCategories);
                foreach (var catId in dto.CategoryIds)
                {
                    _db.VideoCategories.Add(new VideoCategory { VideoId = id, CategoryId = catId });
                }
            }

            // Update tags if provided
            if (dto.TagIds != null)
            {
                _db.VideoTags.RemoveRange(video.VideoTags);
                foreach (var tagId in dto.TagIds)
                {
                    _db.VideoTags.Add(new VideoTag { VideoId = id, TagId = tagId });
                }
            }

            await _db.SaveChangesAsync();

            return await GetVideo(id);
        }

        // DELETE /api/videos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVideo(int id)
        {
            var video = await _db.Videos.FindAsync(id);
            if (video == null) return NotFound();

            _db.Videos.Remove(video);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // GET /api/videos/stats
        [HttpGet("stats")]
        public async Task<ActionResult<VideoStatsDto>> GetStats([FromQuery] DateTime? fromDate = null)
        {
            var from = fromDate ?? DateTime.Now.AddDays(-7);

            var totalVideos = await _db.Videos.CountAsync();
            var premiumVideos = await _db.Videos.CountAsync(v => v.IsPremium);
            var newThisWeek = await _db.Videos.CountAsync(v => v.CreatedAt >= from);

            var stats = await _db.VideoStats.ToListAsync();
            var totalViews = stats.Sum(s => s.ViewCount);
            var totalLikes = stats.Sum(s => s.LikeCount);
            var avgLikes = totalVideos > 0 ? (double)totalLikes / totalVideos : 0;

            return Ok(new VideoStatsDto
            {
                TotalVideos = totalVideos,
                PremiumVideos = premiumVideos,
                PremiumPercent = totalVideos > 0 ? Math.Round((double)premiumVideos / totalVideos * 100, 1) : 0,
                TotalViews = totalViews,
                AvgLikesPerVideo = Math.Round(avgLikes, 0),
                NewVideosThisWeek = newThisWeek
            });
        }
    }
}
