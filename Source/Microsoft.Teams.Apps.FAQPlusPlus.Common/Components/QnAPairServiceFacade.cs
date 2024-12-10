// <copyright file="QnAPairServiceFacade.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::Azure.Search.Documents.Models;
    using global::Azure.AI.Language.QuestionAnswering;
    using global::Azure.Search.Documents;
    using System.Text.RegularExpressions;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Properties;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.TeamsActivity;
    using Newtonsoft.Json.Linq;
    using System.IO;
    using Newtonsoft.Json;
    using global::Azure.AI.OpenAI;

    /// <summary>
    /// Class that handles get/add/update of QnA pairs.
    /// </summary>
    public class QnAPairServiceFacade : IQnAPairServiceFacade
    {
        private readonly ILogger<QnAPairServiceFacade> logger;
        private readonly string appBaseUri;
        private readonly BotSettings options;

        private SearchClient srchClient;
        private OpenAIClient openAIClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAPairServiceFacade"/> class.
        /// </summary>
        /// <param name="botSettings">Represents a set of key/value application configuration properties for FaqPlusPlus bot.</param>ram>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        public QnAPairServiceFacade(
            IOptionsMonitor<BotSettings> botSettings,
            ILogger<QnAPairServiceFacade> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (botSettings == null)
            {
                throw new ArgumentNullException(nameof(botSettings));
            }

            this.options = botSettings.CurrentValue;
            this.appBaseUri = this.options.AppBaseUri;

            // Search service instance
            Uri serviceEndpoint = new Uri($"https://" + botSettings.CurrentValue.SEARCH_SERVICE_NAME + ".search.windows.net/");
            global::Azure.AzureKeyCredential credential = new global::Azure.AzureKeyCredential(botSettings.CurrentValue.SEARCH_QUERY_KEY);
            this.srchClient = new SearchClient(serviceEndpoint, botSettings.CurrentValue.SEARCH_INDEX_NAME, credential);


            // OpenAIClient instance
            var endpoint = new Uri(botSettings.CurrentValue.AOAI_ENDPOINT);
            var credentials = new global::Azure.AzureKeyCredential(botSettings.CurrentValue.AOAI_KEY);
            this.openAIClient = new OpenAIClient(endpoint, credentials);
        }

        private static string ReplaceRelativeMarkDownLinks(string str)
        {
            string pattern = @"\[(.*?)\]\(\/[^\)]*\)";
            return Regex.Replace(str, pattern, m =>
            {
                string linkText = m.Groups[1].Value;
                string url = m.Value.Substring(m.Value.IndexOf('(') + 1, m.Value.LastIndexOf(')') - m.Value.IndexOf('(') - 1);
                return $"[{linkText}](https://confluence.intertech.com.tr{url})";
            });
        }

        /// <summary>
        /// Get the reply to a question asked by end user.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="message">Text message.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task GetReplyToQnAAsync(
            ITurnContext<IMessageActivity> turnContext,
            IMessageActivity message)
        {
            string text = message.Text?.Trim() ?? string.Empty;
            string userName = "UserName:";
            if (message.From != null && message.From.Name != null) {
                userName = message.From.Name + "-";
                userName += message.From.AadObjectId;
            }

            ResponseCardPayload payload = new ResponseCardPayload();

            if (!string.IsNullOrEmpty(message.ReplyToId) && (message.Value != null))
            {
                payload = ((JObject)message.Value).ToObject<ResponseCardPayload>();
            }

            var answer = await ConsolidatedAnswer(text, userName);
            IMessageActivity messageActivity = MessageFactory.Attachment(ResponseCard.GetCard(answer, text, this.appBaseUri, payload));
            messageActivity.TextFormat = "markdown";
            await turnContext.SendActivityAsync(messageActivity).ConfigureAwait(false);
        }

        private async Task<string> ConsolidatedAnswer(string userMessage, string userName )
        {
            var question = userMessage;
            var context = await GetSearchResult(question);
            var promptText = CreateQuestionAndContext(question, context, userName);
            var responseFromGPT = await GetAnswerFromGPT(promptText);
            return responseFromGPT;
        }


        // Function to generate embeddings
        private static async Task<IReadOnlyList<float>> GenerateEmbeddings(string text, OpenAIClient openAIClient, BotSettings options)
        {
            // TODO optionsModelName ekle
            // var response = await openAIClient.GetEmbeddingsAsync(options.AOAI_EmbeddingModelName, new EmbeddingsOptions(text));

            var response = await openAIClient.GetEmbeddingsAsync(options.AOAI_EmbeddingModelName, new EmbeddingsOptions(text));
            return response.Value.Data[0].Embedding;
        }

        // caglar - search query
        public async Task<string> GetSearchResult(string searchQuery, int? count = null, int? skip = null)
        {
            try
            {
                this.logger.Log(LogLevel.Information, "SearchQuery:" + searchQuery);
                var queryEmbeddings = await GenerateEmbeddings(searchQuery, this.openAIClient,this.options);
                global::Azure.Search.Documents.SearchOptions searchOptions = new global::Azure.Search.Documents.SearchOptions();
                searchOptions.SemanticSearch = new SemanticSearchOptions() { SemanticConfigurationName = "mergenmarkdown-config" };
                searchOptions.QueryType = global::Azure.Search.Documents.Models.SearchQueryType.Semantic;
                searchOptions.Size = Convert.ToInt32(this.options.SettingForTopK);
                searchOptions.Select.Add("sourceUrl");
                searchOptions.Select.Add("content");
                searchOptions.Select.Add("hierarchy");

                if (this.options.SEARCH_INDEX_NAME.StartsWith("vi-"))
                {
                    searchOptions.VectorSearch = new() { Queries = { new VectorizedQuery(queryEmbeddings.ToArray()) { KNearestNeighborsCount = Convert.ToInt32(this.options.SettingForTopK), Fields = { "contentVector" } } } };
                }

                var returnData = this.srchClient.Search<SearchDocument>(searchQuery, searchOptions);
                string searchResult = string.Empty;

                if (returnData == null)
                {
                    return searchResult;
                }

                var serializer = new JsonSerializer();

                using (var sr = new StreamReader(returnData.GetRawResponse().Content.ToStream()))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    searchResult = "No information was found. Answer the question with your general knowledge. Let the user know that you have not found any information in the knowledge base and are responding with your general knowledge from the internet.";

                    var jsObj = serializer.Deserialize(jsonTextReader) as JObject;
                    var valueSection = jsObj["value"];
                    if (valueSection == null || !valueSection.HasValues)
                    {
                        return searchResult;
                    }

                    var reRankerScore = Convert.ToDecimal(valueSection.Children().First()["@search.rerankerScore"].Value<string>());

                    if (reRankerScore < 1)
                    {
                        return searchResult;
                    }

                    int i = 0;
                    searchResult = string.Empty;

                    foreach (var child in valueSection.Children().OrderByDescending(o => o["@search.rerankerScore"]).Take(Convert.ToInt32(options.SettingForTopK)))
                    {
                        i++;
                        searchResult += "[Result]: <Title>" + child["hierarchy"] + "</Title><Url>" + child["sourceUrl"] + "</Url>\n" + child["content"].Value<string>() + "\n\n";
                    }
                }

                return searchResult;
            }
            catch (Exception ex)
            {
                this.logger.Log(LogLevel.Error, ex.Message);
                return string.Empty;
            }

        }

        private string CreateQuestionAndContext(string question, string context, string username)
        {
            return string.Format("[Question] {0} \r\n\r\n[Context] {1} \r\n", question, context, username);
        }

        public async Task<string> GetAnswerFromGPT(string promptText)
        {
            var today = DateTime.Today;
            var thisWeekStart = today.AddDays(-(int)today.DayOfWeek + 1);
            var thisWeekEnd = thisWeekStart.AddDays(7).AddSeconds(-1);
            var chatMessageAsistant = new ChatMessage(ChatRole.Assistant, string.Format(options.SettingForPrompt, today.ToString("dddd, dd MMMM yyyy"), thisWeekStart.ToString("dddd, dd MMMM yyyy"), thisWeekEnd.ToString("dddd, dd MMMM yyyy")));
            var chatMessageUser = new ChatMessage(ChatRole.User, promptText);
            var completionOptions = new ChatCompletionsOptions
            {
                Messages = { chatMessageAsistant, chatMessageUser },
                MaxTokens = Convert.ToInt32(this.options.SettingForMaxToken),
                Temperature = float.Parse(this.options.SettingForTemperature),
                FrequencyPenalty = 0.5f,
                PresencePenalty = 0.0f,
                NucleusSamplingFactor = 0.95F,
                StopSequences = { "You:" },
            };

            var response = await this.openAIClient.GetChatCompletionsAsync(this.options.AOAI_DEPLOYMENTID, completionOptions);
            var rawResponse = response.GetRawResponse();
            var responseText = string.Empty;

            if (rawResponse.IsError)
            {
                if (rawResponse.Status == 429)
                {
                    responseText = "Şu anda sistemde bir yoğunluk var. Lütfen bir dakika sonra tekrar deneyin.";
                }
                else
                {
                    responseText = "Beklenmeyen bir hata alındı: " + rawResponse.ReasonPhrase;
                }
            }
            else
            {
                responseText = response.Value.Choices.First().Message.Content;
            }

            this.logger.Log(LogLevel.Information, "Prompt:" + promptText + " - " + "Response:" + responseText);

            return ReplaceRelativeMarkDownLinks(responseText);
        }
    }
}
