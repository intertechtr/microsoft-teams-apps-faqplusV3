using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.Apps.FAQPlusPlus.Models;

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
        public IActionResult Agent([FromHeader(Name = "X-GitHub-Token")] string githubToken, [FromBody] CopilotData copilotData)
        {
            foreach (var message in copilotData.Messages)
            {
                this.logger.LogInformation($"Role: {message.Role}, Content: {message.Content}");
            }

            return Ok("{\"id\": \"1234567890\", \"model\": \"llama2-70b-chat\",  \"choices\": [    {      \"index\": 0,     \"finish_reason\": \"stop\",     \"message\": {        \"role\": \"assistant\",        \"content\": \"Hello World\"      }    }  ],  \"created\": 1234567890,  \"object\": \"chat.completion\",  \"usage\": {    \"prompt_tokens\": 205,    \"completion_tokens\": 5,    \"total_tokens\": 210  }}");
        }

        [HttpGet("callback")]
        public IActionResult Callback()
        {
            string message = "You may close this tab and return to GitHub.com (where you should refresh the page and start a fresh chat). If you're using VS Code or Visual Studio, return there.";
            return Ok(message);
        }
    }
}
