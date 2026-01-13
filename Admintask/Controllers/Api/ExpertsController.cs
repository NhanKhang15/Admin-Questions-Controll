using AdminPortal.Data;
using AdminPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminPortal.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpertsController : ControllerBase
    {
        private readonly FloriaDbContext _db;
        public ExpertsController(FloriaDbContext db) => _db = db;

        // GET /api/experts
        [HttpGet]
        public async Task<ActionResult<List<ExpertSimpleDto>>> GetExperts()
        {
            var experts = await _db.Experts
                .OrderBy(e => e.FullName)
                .Select(e => new ExpertSimpleDto
                {
                    ExpertId = e.ExpertId,
                    Name = e.FullName,
                    AvatarUrl = null  // Not in DB
                })
                .ToListAsync();

            return Ok(experts);
        }

        // GET /api/experts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ExpertSimpleDto>> GetExpert(int id)
        {
            var expert = await _db.Experts.FindAsync(id);
            if (expert == null) return NotFound();

            return Ok(new ExpertSimpleDto
            {
                ExpertId = expert.ExpertId,
                Name = expert.FullName,
                AvatarUrl = null
            });
        }
    }
}
