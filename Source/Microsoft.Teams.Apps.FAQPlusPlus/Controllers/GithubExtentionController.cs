using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Teams.Apps.FAQPlusPlus.Common.Components;
using Microsoft.Teams.Apps.FAQPlusPlus.Models;
using NuGet.Common;
using System.Threading.Tasks;
using System.Text.Json;
using System;

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
        public async Task<IActionResult> Agent([FromHeader(Name = "X-GitHub-Token")] string githubToken, [FromBody] CopilotData copilotData)
        {
            if (copilotData?.Messages == null || !copilotData.Messages.Any())
            {
                return BadRequest(new { error = "No messages provided" });
            }
        
            var lastMessage = copilotData.Messages.LastOrDefault();
            if (lastMessage == null)
            {
                return BadRequest(new { error = "Invalid message format" });
            }
        
            this.logger.LogInformation($"Role: {lastMessage.Role}, Content: {lastMessage.Content}");
        
            var answer = await this.qnaService.ConsolidatedAnswer(lastMessage.Content, "");
        
            var response = new
            {
                id = "chtcmp-" + Guid.NewGuid().ToString(),
                @object = "chat.completion",
                created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                choices = new[]
                {
                    new
                    {
                        index = 0,
                        message = new
                        {
                            role = "assistant",
                            content = answer
                        },
                        finish_reason = "stop"
                    }
                }
            };
        
            return Ok(response);
        }


        [HttpGet("callback")]
        public IActionResult Callback()
        {
            string message = "You may close this tab and return to GitHub.com (where you should refresh the page and start a fresh chat). If you're using VS Code or Visual Studio, return there.";
            return Ok(message);
        }
    }
}
