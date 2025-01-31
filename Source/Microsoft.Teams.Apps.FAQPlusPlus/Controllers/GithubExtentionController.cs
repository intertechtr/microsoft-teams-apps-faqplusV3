using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.Apps.FAQPlusPlus.Common.Components;
using Microsoft.Teams.Apps.FAQPlusPlus.Models;
using NuGet.Common;
using System.Threading.Tasks;
using System.Text.Json;

namespace Microsoft.Teams.Apps.FAQPlusPlus.Controllers
{
    [ApiController]
    [Route("api/github")]
    public class GithubExtentionController : ControllerBase
    {
        private readonly IQnAPairServiceFacade qnaService;
        private readonly ILogger<GithubExtentionController> logger;

        public GithubExtentionController(IQnAPairServiceFacade qnaService, ILogger<GithubExtentionController> logger)
        {
            this.qnaService = qnaService;
            this.logger = logger;
        }

        [HttpPost("agent")]
        public async Task Agent([FromHeader(Name = "X-GitHub-Token")] string githubToken, [FromBody] CopilotData copilotData)
        {
            var msg = "";

            foreach (var message in copilotData.Messages)
            {
                this.logger.LogInformation($"Role: {message.Role}, Content: {message.Content}");
                msg = message.Content;
            }

            var answer = await this.qnaService.ConsolidatedAnswer(msg, "");

            string responseString = $"data: {{\"object\":\"chat.completion.chunk\", \"choices\":[{{\"index\":0,\"delta\":{{\"role\":\"assistant\",\"content\":\"{JsonEncodedText.Encode(answer)}\"}}}}]}}\n\ndata: [DONE]\n\n";

            this.logger.LogInformation($"Response at Github Extension Level: {responseString}");

            await this.Response.WriteAsync(responseString);
            await this.Response.Body.FlushAsync();
        }

        [HttpGet("callback")]
        public IActionResult Callback()
        {
            string message = "You may close this tab and return to GitHub.com (where you should refresh the page and start a fresh chat). If you're using VS Code or Visual Studio, return there.";
            return Ok(message);
        }
    }
}
