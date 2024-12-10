// <copyright file="ConversationService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Components
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Properties;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Class that handles sending welcome card and adaptive card in personal and team chat.
    /// </summary>
    public class ConversationService : IConversationService
    {
        private readonly IQnAPairServiceFacade qnaPairServiceFacade;
        private readonly ILogger<ConversationService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationService"/> class.
        /// </summary>
        /// <param name="configurationProvider">Configuration Provider.</param>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="qnaPairServiceFacade">Instance of QnA pair service class to call add/update/get QnA pair.</param>
        public ConversationService(
            IQnAPairServiceFacade qnaPairServiceFacade,
            ILogger<ConversationService> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.qnaPairServiceFacade = qnaPairServiceFacade ?? throw new ArgumentNullException(nameof(qnaPairServiceFacade));
        }

        /// <summary>
        /// Sends welcome card in 1:1 chat.
        /// </summary>
        /// <param name="membersAdded">Channel account information needed to route a message.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task SendWelcomeCardInPersonalChatAsync(
            IList<ChannelAccount> membersAdded,
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;
            if (membersAdded.Any(channelAccount => channelAccount.Id == activity.Recipient.Id))
            {
                // User started chat with the bot in personal scope, for the first time.
                this.logger.LogInformation($"Bot added to 1:1 chat {activity.Conversation.Id}");
                var welcomeText = "MergenPlus'a hoş geldiniz!";
                var userWelcomeCardAttachment = WelcomeCard.GetCard(welcomeText);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(userWelcomeCardAttachment), cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Handle adaptive card submit in 1:1 chat.
        /// Submits the question or feedback to the SME team.
        /// </summary>
        /// <param name="message">A message in a conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task SendAdaptiveCardInPersonalChatAsync(
            IMessageActivity message,
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            Attachment userCard = null;         // Acknowledgement to the user

            string text = (message.Text ?? string.Empty).Trim();

            switch (text)
            {
                // Sends user the feedback card from the answer card.
                case Constants.ShareFeedback:
                    this.logger.LogInformation("Sending user share feedback card (from answer)");
                    var shareFeedbackPayload = ((JObject)message.Value).ToObject<ResponseCardPayload>();
                    await this.SendActivityInChatAsync(turnContext, MessageFactory.Attachment(ShareFeedbackCard.GetCard(shareFeedbackPayload)), cancellationToken);
                    break;

                default:
                    var payload = ((JObject)message.Value).ToObject<ResponseCardPayload>();

                    if (payload.IsPrompt)
                    {
                        this.logger.LogInformation("Sending input to QuestionAnswer for prompt");
                        await this.qnaPairServiceFacade.GetReplyToQnAAsync(turnContext, message).ConfigureAwait(false);
                    }
                    else
                    {
                        this.logger.LogWarning($"Unexpected text in submit payload: {message.Text}");
                    }

                    break;
            }

            // Send acknowledgment to the user
            if (userCard != null)
            {
                await this.SendActivityInChatAsync(turnContext, MessageFactory.Attachment(userCard), cancellationToken);
            }
        }

        /// <summary>
        /// Sends activity in chat.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="activity">Activity to be sent in chat</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task<ResourceResponse> SendActivityInChatAsync(
            ITurnContext<IMessageActivity> turnContext,
            IActivity activity,
            CancellationToken cancellationToken)
        {
            return await turnContext.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
        }
    }
}
