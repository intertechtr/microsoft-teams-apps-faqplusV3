using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Microsoft.Teams.Apps.FAQPlusPlus.Controllers
{
    [ApiController]
    [Route("api/github")]
    public class GithubExtentionController : ControllerBase
    {
        private readonly ILogger<GithubExtentionController> logger;

        public GithubExtentionController(ILogger<GithubExtentionController> logger)
        {
            this.logger = logger;
        }

        [HttpPost("agent")]
        public IActionResult Agent([FromHeader(Name = "X-GitHub-Token")] string githubToken, [FromBody] Request userRequest)
        {
            // Implement your logic here
            this.logger.LogInformation("GitHubToken = " + githubToken);
            this.logger.LogInformation(userRequest.ToString());
            return Ok("{'message':'Hello World'}");
        }

        [HttpGet("callback")]
        public IActionResult Callback()
        {
            string message = "You may close this tab and return to GitHub.com (where you should refresh the page and start a fresh chat). If you're using VS Code or Visual Studio, return there.";
            return Ok(message);
        }
    }
}
