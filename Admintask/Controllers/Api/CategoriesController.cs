using AdminPortal.Data;
using AdminPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminPortal.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly FloriaDbContext _db;
        public CategoriesController(FloriaDbContext db) => _db = db;

        // GET /api/categories
        [HttpGet]
        public async Task<ActionResult<List<CategorySimpleDto>>> GetCategories([FromQuery] bool? isActive = true)
        {
            var query = _db.ContentCategories.AsQueryable();

            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);

            var categories = await query
                .OrderBy(c => c.Name)
                .Select(c => new CategorySimpleDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Slug = c.Slug
                })
                .ToListAsync();

            return Ok(categories);
        }

        // GET /api/categories/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CategorySimpleDto>> GetCategory(int id)
        {
            var category = await _db.ContentCategories.FindAsync(id);
            if (category == null) return NotFound();

            return Ok(new CategorySimpleDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Slug = category.Slug
            });
        }
    }
}
