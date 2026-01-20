using FlexiRent.Domain.Entities;
using FlexiRent.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.Api.Controllers
{
    [ApiController]
    [Route("api/v1/properties")]
    public class PropertiesController : ControllerBase
    {
        private readonly IGenericRepository<Property> _repo;
        public PropertiesController(IGenericRepository<Property> repo) { _repo = repo; }

        [HttpGet]
        public async Task<IActionResult> GetAll(int skip = 0, int take = 50) => Ok(await _repo.GetAllAsync(skip, take));

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable(int skip = 0, int take = 50) =>
            Ok(await _repo.FindAsync(p => p.IsAvailable, skip, take));

        [HttpGet("owner/{ownerId}")]
        public async Task<IActionResult> ByOwner(Guid ownerId, int skip = 0, int take = 50) =>
            Ok(await _repo.FindAsync(p => p.OwnerId == ownerId, skip, take));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _repo.GetAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] Property prop)
        {
            prop.Id = Guid.NewGuid();
            await _repo.AddAsync(prop);
            return CreatedAtAction(nameof(GetById), new { id = prop.Id }, prop);
        }

        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> Patch(Guid id, [FromBody] Property patch)
        {
            var existing = await _repo.GetAsync(id);
            if (existing == null) return NotFound();
            existing.Title = patch.Title ?? existing.Title;
            existing.Description = patch.Description ?? existing.Description;
            existing.IsAvailable = patch.IsAvailable;
            await _repo.UpdateAsync(existing);
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _repo.GetAsync(id);
            if (existing == null) return NotFound();
            await _repo.DeleteAsync(existing);
            return NoContent();
        }
    }
}
