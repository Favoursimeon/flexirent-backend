using FlexiRent.Infrastructure;
using FlexiRent.Infrastructure.Services;
using FlexiRent.Domain.Entities;
using FlexiRent.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlexiRent.Api.Controllers
{
    [ApiController]
    [Route("api/v1/documents")]
    public class DocumentsController : ControllerBase
    {
        private readonly IFileStorageService _storage;
        private readonly IGenericRepository<Document> _repo;
        public DocumentsController(IFileStorageService storage, IGenericRepository<Document> repo)
        {
            _storage = storage; _repo = repo;
        }

        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] Guid? folderId)
        {
            if (file == null) return BadRequest("file is required");
            var path = await _storage.SaveFileAsync(file, "documents");
            var doc = new Document
            {
                Id = Guid.NewGuid(),
                FolderId = folderId ?? Guid.Empty,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Size = file.Length
            };
            await _repo.AddAsync(doc);
            return Ok(new { document = doc, path });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> ByUser(Guid userId, [FromServices] AppDbContext db)
        {
            var folders = await db.DocumentFolders.Where(f => f.OwnerId == userId).ToListAsync();
            var folderIds = folders.Select(f => f.Id).ToList();
            var docs = await _repo.FindAsync(d => folderIds.Contains(d.FolderId));
            return Ok(docs);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid id, [FromServices] AppDbContext db)
        {
            var doc = await _repo.GetAsync(id);
            if (doc == null) return NotFound();
            // remove file versions too
            await _repo.DeleteAsync(doc);
            return NoContent();
        }
    }
}
