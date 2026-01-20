using FlexiRent.Domain.Entities;
using FlexiRent.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.Api.Controllers
{
    [ApiController]
    [Route("api/v1/reviews")]
    public class ReviewsController : ControllerBase
    {
        private readonly IGenericRepository<Review> _repo;
        public ReviewsController(IGenericRepository<Review> repo) { _repo = repo; }

        [HttpGet("target/{targetType}/{targetId}")]
        public async Task<IActionResult> ByTarget(string targetType, Guid targetId, int skip = 0, int take = 50) =>
            Ok(await _repo.FindAsync(r => r.TargetType == targetType && r.TargetId == targetId, skip, take));

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] Review r)
        {
            r.Id = Guid.NewGuid();
            r.CreatedAt = DateTime.UtcNow;
            await _repo.AddAsync(r);
            return CreatedAtAction(nameof(ByTarget), new { targetType = r.TargetType, targetId = r.TargetId }, r);
        }

        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> Patch(Guid id, [FromBody] Review patch)
        {
            var existing = await _repo.GetAsync(id);
            if (existing == null) return NotFound();
            existing.Rating = patch.Rating != 0 ? patch.Rating : existing.Rating;
            existing.Comment = patch.Comment ?? existing.Comment;
            await _repo.UpdateAsync(existing);
            return Ok(existing);
        }
    }
}
