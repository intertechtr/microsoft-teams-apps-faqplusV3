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
            return Ok("Hello World");
        }

        [HttpGet("callback")]
        public IActionResult Callback()
        {
            string message = "You may close this tab and return to GitHub.com (where you should refresh the page and start a fresh chat). If you're using VS Code or Visual Studio, return there.";
            return Ok(message);
        }
    }
}
