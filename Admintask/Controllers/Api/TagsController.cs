using System.Text.RegularExpressions;
using AdminPortal.Data;
using AdminPortal.Models;
using AdminPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminPortal.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly FloriaDbContext _db;
        public TagsController(FloriaDbContext db) => _db = db;

        // GET /api/tags
        [HttpGet]
        public async Task<ActionResult<List<TagSimpleDto>>> GetTags([FromQuery] string? search = null)
        {
            var query = _db.Tags.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(t => t.Name.Contains(search));

            var tags = await query
                .OrderBy(t => t.Name)
                .Select(t => new TagSimpleDto
                {
                    TagId = t.TagId,
                    Name = t.Name,
                    Slug = t.Slug
                })
                .ToListAsync();

            return Ok(tags);
        }

        // GET /api/tags/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TagSimpleDto>> GetTag(int id)
        {
            var tag = await _db.Tags.FindAsync(id);
            if (tag == null) return NotFound();

            return Ok(new TagSimpleDto
            {
                TagId = tag.TagId,
                Name = tag.Name,
                Slug = tag.Slug
            });
        }

        // POST /api/tags
        [HttpPost]
        public async Task<ActionResult<TagSimpleDto>> CreateTag([FromBody] TagCreateDto dto)
        {
            var slug = GenerateSlug(dto.Name);

            // Check if slug exists
            var existing = await _db.Tags.FirstOrDefaultAsync(t => t.Slug == slug);
            if (existing != null)
            {
                return Ok(new TagSimpleDto
                {
                    TagId = existing.TagId,
                    Name = existing.Name,
                    Slug = existing.Slug
                });
            }

            var tag = new Tag
            {
                Name = dto.Name.Trim(),
                Slug = slug,
                CreatedAt = DateTime.Now
            };

            _db.Tags.Add(tag);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTag), new { id = tag.TagId }, new TagSimpleDto
            {
                TagId = tag.TagId,
                Name = tag.Name,
                Slug = tag.Slug
            });
        }

        private static string GenerateSlug(string text)
        {
            // Remove diacritics and convert to lowercase
            var slug = text.ToLowerInvariant().Trim();
            // Replace spaces with hyphens
            slug = Regex.Replace(slug, @"\s+", "-");
            // Remove invalid characters
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
            // Remove multiple hyphens
            slug = Regex.Replace(slug, @"-+", "-");
            return slug.Trim('-');
        }
    }

    public class TagCreateDto
    {
        public string Name { get; set; } = "";
    }
}
