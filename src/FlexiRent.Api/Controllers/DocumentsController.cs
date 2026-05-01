using System.Security.Claims;
using FlexiRent.Application.DTOs;
using FlexiRent.Application.Models;
using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.API.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documents;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public DocumentsController(IDocumentService documents) => _documents = documents;

    // Folders
    [HttpPost("folders")]
    public async Task<IActionResult> CreateFolder(CreateFolderDto dto) =>
        Ok(await _documents.CreateFolderAsync(UserId, dto));

    [HttpGet("folders")]
    public async Task<IActionResult> GetFolders() =>
        Ok(await _documents.GetFoldersAsync(UserId));

    [HttpPut("folders/{id:guid}")]
    public async Task<IActionResult> UpdateFolder(Guid id, UpdateFolderDto dto) =>
        Ok(await _documents.UpdateFolderAsync(UserId, id, dto));

    [HttpDelete("folders/{id:guid}")]
    public async Task<IActionResult> DeleteFolder(Guid id)
    {
        await _documents.DeleteFolderAsync(UserId, id);
        return NoContent();
    }

    // Documents
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] UploadDocumentDto dto) =>
        Ok(await _documents.UploadDocumentAsync(UserId, dto));

    [HttpGet]
    public async Task<IActionResult> GetDocuments([FromQuery] Guid? folderId) =>
        Ok(await _documents.GetDocumentsAsync(UserId, folderId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDocument(Guid id) =>
        Ok(await _documents.GetDocumentAsync(UserId, id));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateDocument(Guid id, UpdateDocumentDto dto) =>
        Ok(await _documents.UpdateDocumentAsync(UserId, id, dto));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        await _documents.DeleteDocumentAsync(UserId, id);
        return NoContent();
    }

    // Versions
    [HttpPost("{id:guid}/versions")]
    public async Task<IActionResult> UploadVersion(Guid id, IFormFile file, [FromQuery] string? changeNotes)
    {
        var upload = new FileUpload
        {
            Content = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length
        };
        return Ok(await _documents.UploadVersionAsync(UserId, id, upload, changeNotes));
    }

    // Sharing
    [HttpPost("{id:guid}/share")]
    public async Task<IActionResult> Share(Guid id, ShareDocumentDto dto) =>
        Ok(await _documents.ShareDocumentAsync(UserId, id, dto));

    [HttpDelete("shares/{shareId:guid}")]
    public async Task<IActionResult> RevokeShare(Guid shareId)
    {
        await _documents.RevokeShareAsync(UserId, shareId);
        return NoContent();
    }

    [HttpGet("shared-with-me")]
    public async Task<IActionResult> SharedWithMe() =>
        Ok(await _documents.GetSharedWithMeAsync(UserId));

    [HttpGet("shared-by-me")]
    public async Task<IActionResult> SharedByMe() =>
        Ok(await _documents.GetSharedByMeAsync(UserId));
}