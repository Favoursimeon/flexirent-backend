using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.Api.Controllers
{
    [ApiController]
    [Route("api/v1/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboard;
        public DashboardController(IDashboardService dashboard) { _dashboard = dashboard; }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<IActionResult> Admin() => Ok(await _dashboard.GetAdminStatsAsync());

        [Authorize]
        [HttpGet("client/{userId}")]
        public async Task<IActionResult> Client(Guid userId) => Ok(await _dashboard.GetClientStatsAsync(userId));

        [Authorize]
        [HttpGet("vendor/{userId}")]
        public async Task<IActionResult> Vendor(Guid userId) => Ok(await _dashboard.GetVendorStatsAsync(userId));

        [Authorize]
        [HttpGet("provider/{userId}")]
        public async Task<IActionResult> Provider(Guid userId) => Ok(await _dashboard.GetProviderStatsAsync(userId));
    }
}
