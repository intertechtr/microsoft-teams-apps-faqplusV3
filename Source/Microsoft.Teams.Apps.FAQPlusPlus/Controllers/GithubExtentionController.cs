using Microsoft.AspNetCore.Mvc;
using Microsoft.Teams.Apps.FAQPlusPlus.Models;

namespace Microsoft.Teams.Apps.FAQPlusPlus.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GithubExtentionController : ControllerBase
    {
        [HttpPost("agent")]
        public IActionResult Agent([FromHeader(Name = "X-GitHub-Token")] string githubToken, [FromBody] Request userRequest)
        {
            // Implement your logic here
            return Ok();
        }
    }
}
