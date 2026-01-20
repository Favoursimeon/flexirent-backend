using FlexiRent.Domain.Entities;
using FlexiRent.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlexiRent.Api.Controllers
{
    [ApiController]
    [Route("api/v1/search")]
    public class SearchController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public SearchController(ApplicationDbContext db) { _db = db; }

        [HttpGet("properties")]
        public async Task<IActionResult> SearchProperties([FromQuery] string q, decimal? minPrice = null, decimal? maxPrice = null, int skip = 0, int take = 20)
        {
            var query = _db.Properties.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q)) query = query.Where(p => p.Title.Contains(q) || p.Description.Contains(q));
            if (minPrice.HasValue) query = query.Where(p => p.PricePerMonth >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.PricePerMonth <= maxPrice.Value);
            var results = await query.Skip(skip).Take(take).ToListAsync();
            return Ok(results);
        }

        [HttpGet("vendors")]
        public async Task<IActionResult> SearchVendors([FromQuery] string q)
        {
            var vendors = await _db.VendorRegistrations.Where(v => v.BusinessName.Contains(q) || v.Details.Contains(q))
                .Take(50).ToListAsync();
            return Ok(vendors);
        }

        [HttpGet("providers")]
        public async Task<IActionResult> SearchProviders([FromQuery] string q)
        {
            var providers = await _db.ServiceProviderRegistrations.Where(p => p.ServiceType.Contains(q) || p.Details.Contains(q))
                .Take(50).ToListAsync();
            return Ok(providers);
        }
    }
}
