using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.Api.Controllers
{
    [ApiController]
    [Route("api/v1/ratings")]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _rating;
        public RatingsController(IRatingService rating) { _rating = rating; }

        [HttpPost("average")]
        public async Task<IActionResult> Average([FromBody] RatingRequest req)
        {
            var avg = await _rating.GetAverageAsync(req.TargetType, req.TargetId);
            return Ok(new { average = avg });
        }

        [HttpPost("count")]
        public async Task<IActionResult> Count([FromBody] RatingRequest req)
        {
            var count = await _rating.GetCountAsync(req.TargetType, req.TargetId);
            return Ok(new { count });
        }

        public class RatingRequest
        {
            public string TargetType { get; set; }
            public Guid TargetId { get; set; }
        }
    }
}
