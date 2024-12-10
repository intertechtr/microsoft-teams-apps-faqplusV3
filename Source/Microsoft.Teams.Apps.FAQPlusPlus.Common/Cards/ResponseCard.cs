// <copyright file="ResponseCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Cards
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using AdaptiveCards;
    using global::Azure.AI.Language.QuestionAnswering;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Properties;
    using Newtonsoft.Json;

    /// <summary>
    ///  This class process Response Card- Response by bot when user asks a question to bot.
    /// </summary>
    public static class ResponseCard
    {
        /// <summary>
        /// Represent response card icon width in pixel.
        /// </summary>
        private const uint IconWidth = 32;

        /// <summary>
        /// Represent response card icon height in pixel.
        /// </summary>
        private const uint IconHeight = 32;

        /// <summary>
        /// Construct the response card - when user asks a question to the Question Answering through the bot.
        /// </summary>
        /// <param name="response">The response model.</param>
        /// <param name="userQuestion">Actual question that the user has asked the bot.</param>
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <param name="payload">The response card payload.</param>
        /// <returns>The response card to append to a message as an attachment.</returns>
        public static Attachment GetCard(KnowledgeBaseAnswer response, string userQuestion, string appBaseUri, ResponseCardPayload payload)
        {
            bool isRichCard = false;
            AdaptiveSubmitActionData answerModel = new AdaptiveSubmitActionData();
            if (Validators.IsValidJSON(response.Answer))
            {
                answerModel = JsonConvert.DeserializeObject<AdaptiveSubmitActionData>(response.Answer);
                isRichCard = Validators.IsRichCard(answerModel);
            }

            string answer = isRichCard ? answerModel.Description : response.Answer;
            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = BuildResponseCardBody(response, userQuestion, answer, appBaseUri, payload, isRichCard),
                Actions = BuildListOfActions(userQuestion, answer),
            };

            if (!string.IsNullOrEmpty(answerModel.RedirectionUrl))
            {
                responseCard.Actions.Add(
                    new AdaptiveOpenUrlAction
                    {
                        Title = Strings.OpenArticle,
                        Url = new Uri(answerModel.RedirectionUrl),
                    });
            }

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }


        public static Attachment GetCard(string answer, string userQuestion, string appBaseUri, ResponseCardPayload payload)
        {
            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = BuildResponseCardBody(userQuestion, answer, appBaseUri, payload,true),
                Actions = BuildListOfActions(userQuestion, answer),
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }

        /// <summary>
        /// This method builds the body of the response card, and helps to render the follow up prompts if the response contains any.
        /// </summary>
        /// <param name="response">The QnA response model.</param>
        /// /// <param name="userQuestion">The user question - the actual question asked to the bot.</param>
        /// <param name="answer">The answer string.</param>
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <param name="payload">The response card payload.</param>
        /// <param name="isRichCard">Boolean value where true represent it is a rich card while false represent it is a normal card.</param>
        /// <returns>A list of adaptive elements which makes up the body of the adaptive card.</returns>
        private static List<AdaptiveElement> BuildResponseCardBody(KnowledgeBaseAnswer response, string userQuestion, string answer, string appBaseUri, ResponseCardPayload payload, bool isRichCard)
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;
            var answerModel = isRichCard ? JsonConvert.DeserializeObject<AnswerModel>(response?.Answer) : new AnswerModel();

            var cardBodyToConstruct = new List<AdaptiveElement>()
            {
                new AdaptiveTextBlock
                {
                    Wrap = true,
                    Text = answerModel.Title ?? string.Empty,
                    Size = AdaptiveTextSize.Large,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = textAlignment,
                },
                new AdaptiveTextBlock
                {
                    Text = answerModel.Subtitle ?? string.Empty,
                    Size = AdaptiveTextSize.Medium,
                    HorizontalAlignment = textAlignment,
                },
            };

            if (!string.IsNullOrWhiteSpace(answerModel?.ImageUrl))
            {
                cardBodyToConstruct.Add(new AdaptiveImage
                {
                    Url = new Uri(answerModel.ImageUrl.Trim()),
                    Size = AdaptiveImageSize.Auto,
                    Style = AdaptiveImageStyle.Default,
                    AltText = answerModel.Title,
                    IsVisible = isRichCard,
                });
            }

            cardBodyToConstruct.Add(new AdaptiveTextBlock
            {
                Text = answer,
                Wrap = true,
                Size = isRichCard ? AdaptiveTextSize.Small : AdaptiveTextSize.Default,
                Spacing = AdaptiveSpacing.Medium,
                HorizontalAlignment = textAlignment,
            });

            return cardBodyToConstruct;
        }

        private static List<AdaptiveElement> BuildResponseCardBody(string userQuestion, string answer, string appBaseUri, ResponseCardPayload payload, bool isRichCard)
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;

            var cardBodyToConstruct = new List<AdaptiveElement>()
            {
                new AdaptiveTextBlock
                {
                    Text = string.Empty,
                    Size = AdaptiveTextSize.Medium,
                    HorizontalAlignment = textAlignment,
                },
            };

            cardBodyToConstruct.Add(new AdaptiveTextBlock
            {
                Text = answer,
                Wrap = true,
                Size = isRichCard ? AdaptiveTextSize.Small : AdaptiveTextSize.Default,
                Spacing = AdaptiveSpacing.Medium,
                HorizontalAlignment = textAlignment,
            });

            return cardBodyToConstruct;
        }


        /// <summary>
        /// This method will build the necessary list of actions.
        /// </summary>
        /// <param name="userQuestion">The user question - the actual question asked to the bot.</param>
        /// <param name="answer">The answer string.</param>
        /// <returns>A list of adaptive actions.</returns>
        private static List<AdaptiveAction> BuildListOfActions(string userQuestion, string answer)
        {
            List<AdaptiveAction> actionsList = new List<AdaptiveAction>();
            /*{
                // Adds the "Share feedback" button.
                new AdaptiveSubmitAction
                {
                    Title = Strings.ShareFeedbackButtonText,
                    Data = new ResponseCardPayload
                    {
                        MsTeams = new CardAction
                        {
                            Type = ActionTypes.MessageBack,
                            DisplayText = Strings.ShareFeedbackDisplayText,
                            Text = Constants.ShareFeedback,
                        },
                        UserQuestion = userQuestion,
                        KnowledgeBaseAnswer = answer,
                    },
                },
            };*/

            return actionsList;
        }
    }
}
