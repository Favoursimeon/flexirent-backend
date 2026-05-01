using System.Security.Claims;
using FlexiRent.Application.DTOs;
using FlexiRent.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.API.Controllers;

[ApiController]
[Route("api/portfolio")]
[Authorize]
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioService _portfolio;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public PortfolioController(IPortfolioService portfolio) => _portfolio = portfolio;

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] UploadPortfolioImageDto dto) =>
        Ok(await _portfolio.UploadImageAsync(UserId, dto));

    [HttpGet]
    public async Task<IActionResult> GetImages() =>
        Ok(await _portfolio.GetImagesAsync(UserId));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdatePortfolioImageDto dto) =>
        Ok(await _portfolio.UpdateImageAsync(UserId, id, dto));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _portfolio.DeleteImageAsync(UserId, id);
        return NoContent();
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder([FromBody] List<Guid> orderedIds)
    {
        await _portfolio.ReorderAsync(UserId, orderedIds);
        return NoContent();
    }
}