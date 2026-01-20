using FlexiRent.Domain.Entities;
using FlexiRent.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.Api.Controllers
{
    [ApiController]
    [Route("api/v1/bookings")]
    public class BookingsController : ControllerBase
    {
        private readonly IGenericRepository<Booking> _repo;
        public BookingsController(IGenericRepository<Booking> repo) { _repo = repo; }

        [HttpGet]
        public async Task<IActionResult> GetAll(int skip = 0, int take = 50) => Ok(await _repo.GetAllAsync(skip, take));

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> ByUser(Guid userId, int skip = 0, int take = 50) =>
            Ok(await _repo.FindAsync(b => b.UserId == userId, skip, take));

        [HttpGet("provider/{providerId}")]
        public async Task<IActionResult> ByProvider(Guid providerId, int skip = 0, int take = 50) =>
            Ok(await _repo.FindAsync(b => b.ProviderId == providerId, skip, take));

        [HttpGet("status/{status}")]
        public async Task<IActionResult> ByStatus(string status, int skip = 0, int take = 50) =>
            Ok(await _repo.FindAsync(b => b.Status == status, skip, take));

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] Booking b)
        {
            b.Id = Guid.NewGuid();
            b.CreatedAt = DateTime.UtcNow;
            await _repo.AddAsync(b);
            return CreatedAtAction(nameof(GetAll), new { id = b.Id }, b);
        }

        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> Patch(Guid id, [FromBody] Booking patch)
        {
            var existing = await _repo.GetAsync(id);
            if (existing == null) return NotFound();
            existing.Status = patch.Status ?? existing.Status;
            existing.StartAt = patch.StartAt == default ? existing.StartAt : patch.StartAt;
            existing.EndAt = patch.EndAt == default ? existing.EndAt : patch.EndAt;
            await _repo.UpdateAsync(existing);
            return Ok(existing);
        }
    }
}
