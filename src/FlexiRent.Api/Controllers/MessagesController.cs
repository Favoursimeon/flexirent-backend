using FlexiRent.Domain.Entities;
using FlexiRent.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.Api.Controllers
{
    [ApiController]
    [Route("api/v1/messages")]
    public class MessagesController : ControllerBase
    {
        private readonly IGenericRepository<Message> _repo;
        public MessagesController(IGenericRepository<Message> repo) { _repo = repo; }

        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> ByBooking(Guid bookingId, int skip = 0, int take = 200) =>
            Ok(await _repo.FindAsync(m => m.BookingId == bookingId, skip, take));

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Send([FromBody] Message message)
        {
            message.Id = Guid.NewGuid();
            message.SentAt = DateTime.UtcNow;
            await _repo.AddAsync(message);
            return CreatedAtAction(nameof(ByBooking), new { bookingId = message.BookingId }, message);
        }

        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> Patch(Guid id, [FromBody] Message patch)
        {
            var existing = await _repo.GetAsync(id);
            if (existing == null) return NotFound();
            existing.IsRead = patch.IsRead;
            existing.Body = patch.Body ?? existing.Body;
            await _repo.UpdateAsync(existing);
            return Ok(existing);
        }
    }
}
