// <copyright file="BotCommandResolver.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Components
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Properties;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Class that resolves bot commands in personal and teams chat.
    /// </summary>
    public class BotCommandResolver : IBotCommandResolver
    {
        private readonly ILogger<BotCommandResolver> logger;
        private readonly IQnAPairServiceFacade qnaPairService;
        private readonly string appBaseUri;
        private readonly IConversationService conversationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotCommandResolver"/> class.
        /// </summary>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="qnaPairService">Instance of QnA pair service class to call add/update/get QnA pair.</param>
        /// <param name="botSettings">Represents a set of key/value application configuration properties for FaqPlusPlus bot.</param>
        /// <param name="conversationService">Conversation service to send adaptive card in personal and teams chat.</param>
        public BotCommandResolver(
            IQnAPairServiceFacade qnaPairService,
            IOptionsMonitor<BotSettings> botSettings,
            ILogger<BotCommandResolver> logger,
            IConversationService conversationService)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.qnaPairService = qnaPairService ?? throw new ArgumentNullException(nameof(qnaPairService));
            this.conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
            if (botSettings == null)
            {
                throw new ArgumentNullException(nameof(botSettings));
            }

            var options = botSettings.CurrentValue;
            this.appBaseUri = options.AppBaseUri;
        }

        /// <summary>
        /// Resolve bot command in 1:1 chat.
        /// </summary>
        /// <param name="message">A message in a conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task ResolveBotCommandInPersonalChatAsync(
            IMessageActivity message,
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(message.ReplyToId) && (message.Value != null) && ((JObject)message.Value).HasValues)
            {
                this.logger.LogInformation("Card submit in 1:1 chat");
                await this.conversationService.SendAdaptiveCardInPersonalChatAsync(message, turnContext, cancellationToken).ConfigureAwait(false);
                return;
            }

            string text = (message.Text ?? string.Empty).Trim();

            if (text.Equals(Strings.BotCommandFeedback, StringComparison.CurrentCultureIgnoreCase) ||
                text.Equals(Constants.ShareFeedback, StringComparison.InvariantCultureIgnoreCase))
            {
                this.logger.LogInformation("Sending user feedback card");
                await turnContext.SendActivityAsync(MessageFactory.Attachment(ShareFeedbackCard.GetCard())).ConfigureAwait(false);
            }
            else if (text.Equals(Strings.BotCommandTour, StringComparison.CurrentCultureIgnoreCase) ||
                text.Equals(Constants.TakeATour, StringComparison.InvariantCultureIgnoreCase))
            {
                this.logger.LogInformation("Sending user tour card");
                var userTourCards = TourCarousel.GetUserTourCards(this.appBaseUri);
                await turnContext.SendActivityAsync(MessageFactory.Carousel(userTourCards)).ConfigureAwait(false);
            }
            else
            {
                this.logger.LogInformation("Sending input to QuestionAnswer");
                await this.qnaPairService.GetReplyToQnAAsync(turnContext, message).ConfigureAwait(false);
            }
        }
    }
}
