using FlexiRent.Domain.Entities;
using FlexiRent.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.Api.Controllers
{
    [ApiController]
    [Route("api/v1/serviceproviderregistrations")]
    public class ServiceProviderRegistrationsController : ControllerBase
    {
        private readonly IGenericRepository<ServiceProviderRegistration> _repo;
        public ServiceProviderRegistrationsController(IGenericRepository<ServiceProviderRegistration> repo) { _repo = repo; }

        [HttpGet]
        public async Task<IActionResult> GetAll(int skip = 0, int take = 50)
        {
            var list = await _repo.GetAllAsync(skip, take);
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _repo.GetAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(string status, int skip = 0, int take = 50)
        {
            var items = await _repo.FindAsync(x => x.Status == status, skip, take);
            return Ok(items);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ServiceProviderRegistration model)
        {
            model.Id = Guid.NewGuid();
            model.CreatedAt = DateTime.UtcNow;
            await _repo.AddAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Patch(Guid id, [FromBody] ServiceProviderRegistration patch)
        {
            var existing = await _repo.GetAsync(id);
            if (existing == null) return NotFound();

            existing.Status = patch.Status ?? existing.Status;
            existing.Details = patch.Details ?? existing.Details;
            await _repo.UpdateAsync(existing);
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _repo.GetAsync(id);
            if (existing == null) return NotFound();
            await _repo.DeleteAsync(existing);
            return NoContent();
        }
    }
}
