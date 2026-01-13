using AdminPortal.Data;
using AdminPortal.Models;
using AdminPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminPortal.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly FloriaDbContext _db;
        public PostsController(FloriaDbContext db) => _db = db;

        // GET /api/posts
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<PostDto>>> GetPosts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] bool? isPremium = null,
            [FromQuery] string? status = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] int? expertId = null)
        {
            var query = _db.Posts
                .Include(p => p.Expert)
                .Include(p => p.PostCategories).ThenInclude(pc => pc.Category)
                .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.Stats)
                .AsQueryable();

            // Filters
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Title.Contains(search) || (p.Summary != null && p.Summary.Contains(search)));

            if (isPremium.HasValue)
                query = query.Where(p => p.IsPremium == isPremium.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (categoryId.HasValue)
                query = query.Where(p => p.PostCategories.Any(pc => pc.CategoryId == categoryId.Value));

            if (expertId.HasValue)
                query = query.Where(p => p.ExpertId == expertId.Value);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PostDto
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    Summary = p.Summary,
                    Content = p.Content,
                    ThumbnailUrl = p.ThumbnailUrl,
                    IsPremium = p.IsPremium,
                    Status = p.Status,
                    PublishedAt = p.PublishedAt,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Expert = p.Expert != null ? new ExpertSimpleDto
                    {
                        ExpertId = p.Expert.ExpertId,
                        Name = p.Expert.FullName,
                        AvatarUrl = null
                    } : null,
                    Categories = p.PostCategories.Select(pc => new CategorySimpleDto
                    {
                        CategoryId = pc.Category.CategoryId,
                        Name = pc.Category.Name,
                        Slug = pc.Category.Slug
                    }).ToList(),
                    Tags = p.PostTags.Select(pt => new TagSimpleDto
                    {
                        TagId = pt.Tag.TagId,
                        Name = pt.Tag.Name,
                        Slug = pt.Tag.Slug
                    }).ToList(),
                    Stats = p.Stats != null ? new ContentStatsDto
                    {
                        ViewCount = p.Stats.ViewCount,
                        LikeCount = p.Stats.LikeCount
                    } : null
                })
                .ToListAsync();

            return Ok(new PaginatedResult<PostDto>
            {
                Data = posts,
                Pagination = new PaginationInfo
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                }
            });
        }

        // GET /api/posts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PostDto>> GetPost(int id)
        {
            var p = await _db.Posts
                .Include(x => x.Expert)
                .Include(x => x.PostCategories).ThenInclude(pc => pc.Category)
                .Include(x => x.PostTags).ThenInclude(pt => pt.Tag)
                .Include(x => x.Stats)
                .FirstOrDefaultAsync(x => x.PostId == id);

            if (p == null) return NotFound();

            return Ok(new PostDto
            {
                PostId = p.PostId,
                Title = p.Title,
                Summary = p.Summary,
                Content = p.Content,
                ThumbnailUrl = p.ThumbnailUrl,
                IsPremium = p.IsPremium,
                Status = p.Status,
                PublishedAt = p.PublishedAt,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Expert = p.Expert != null ? new ExpertSimpleDto
                {
                    ExpertId = p.Expert.ExpertId,
                    Name = p.Expert.FullName,
                    AvatarUrl = null
                } : null,
                Categories = p.PostCategories.Select(pc => new CategorySimpleDto
                {
                    CategoryId = pc.Category.CategoryId,
                    Name = pc.Category.Name,
                    Slug = pc.Category.Slug
                }).ToList(),
                Tags = p.PostTags.Select(pt => new TagSimpleDto
                {
                    TagId = pt.Tag.TagId,
                    Name = pt.Tag.Name,
                    Slug = pt.Tag.Slug
                }).ToList(),
                Stats = p.Stats != null ? new ContentStatsDto
                {
                    ViewCount = p.Stats.ViewCount,
                    LikeCount = p.Stats.LikeCount
                } : null
            });
        }

        // POST /api/posts
        [HttpPost]
        public async Task<ActionResult<PostDto>> CreatePost([FromBody] PostCreateDto dto)
        {
            var post = new Post
            {
                ExpertId = dto.ExpertId,
                Title = dto.Title,
                Summary = dto.Summary,
                Content = dto.Content,
                ThumbnailUrl = dto.ThumbnailUrl,
                IsPremium = dto.IsPremium,
                Status = dto.Status,
                PublishedAt = dto.PublishedAt,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _db.Posts.Add(post);
            await _db.SaveChangesAsync();

            // Add categories
            foreach (var catId in dto.CategoryIds)
            {
                _db.PostCategories.Add(new PostCategory { PostId = post.PostId, CategoryId = catId });
            }

            // Add tags
            foreach (var tagId in dto.TagIds)
            {
                _db.PostTags.Add(new PostTag { PostId = post.PostId, TagId = tagId });
            }

            // Create stats record
            _db.PostStats.Add(new PostStats { PostId = post.PostId });

            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPost), new { id = post.PostId }, await GetPost(post.PostId));
        }

        // PUT /api/posts/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<PostDto>> UpdatePost(int id, [FromBody] PostUpdateDto dto)
        {
            var post = await _db.Posts
                .Include(p => p.PostCategories)
                .Include(p => p.PostTags)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null) return NotFound();

            // Update fields if provided
            if (dto.ExpertId.HasValue) post.ExpertId = dto.ExpertId;
            if (dto.Title != null) post.Title = dto.Title;
            if (dto.Summary != null) post.Summary = dto.Summary;
            if (dto.Content != null) post.Content = dto.Content;
            if (dto.ThumbnailUrl != null) post.ThumbnailUrl = dto.ThumbnailUrl;
            if (dto.IsPremium.HasValue) post.IsPremium = dto.IsPremium.Value;
            if (dto.Status != null) post.Status = dto.Status;
            if (dto.PublishedAt.HasValue) post.PublishedAt = dto.PublishedAt;

            post.UpdatedAt = DateTime.Now;

            // Update categories if provided
            if (dto.CategoryIds != null)
            {
                _db.PostCategories.RemoveRange(post.PostCategories);
                foreach (var catId in dto.CategoryIds)
                {
                    _db.PostCategories.Add(new PostCategory { PostId = id, CategoryId = catId });
                }
            }

            // Update tags if provided
            if (dto.TagIds != null)
            {
                _db.PostTags.RemoveRange(post.PostTags);
                foreach (var tagId in dto.TagIds)
                {
                    _db.PostTags.Add(new PostTag { PostId = id, TagId = tagId });
                }
            }

            await _db.SaveChangesAsync();

            return await GetPost(id);
        }

        // DELETE /api/posts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _db.Posts.FindAsync(id);
            if (post == null) return NotFound();

            _db.Posts.Remove(post);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // GET /api/posts/stats
        [HttpGet("stats")]
        public async Task<ActionResult<PostStatsDto>> GetStats([FromQuery] DateTime? fromDate = null)
        {
            var from = fromDate ?? DateTime.Now.AddDays(-7);

            var totalPosts = await _db.Posts.CountAsync();
            var premiumPosts = await _db.Posts.CountAsync(p => p.IsPremium);
            var newThisWeek = await _db.Posts.CountAsync(p => p.CreatedAt >= from);

            var stats = await _db.PostStats.ToListAsync();
            var totalViews = stats.Sum(s => s.ViewCount);
            var totalLikes = stats.Sum(s => s.LikeCount);
            var avgLikes = totalPosts > 0 ? (double)totalLikes / totalPosts : 0;

            return Ok(new PostStatsDto
            {
                TotalPosts = totalPosts,
                PremiumPosts = premiumPosts,
                PremiumPercent = totalPosts > 0 ? Math.Round((double)premiumPosts / totalPosts * 100, 1) : 0,
                TotalViews = totalViews,
                AvgLikesPerPost = Math.Round(avgLikes, 0),
                NewPostsThisWeek = newThisWeek
            });
        }
    }
}
