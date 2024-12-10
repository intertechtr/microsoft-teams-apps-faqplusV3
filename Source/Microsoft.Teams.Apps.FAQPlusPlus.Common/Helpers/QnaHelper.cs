// <copyright file="QnaHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Azure.AI.Language.QuestionAnswering;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Properties;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Qna helper class for qna pair data.
    /// </summary>
    public static class QnaHelper
    {
        /// <summary>
        /// Get combined description for rich card.
        /// </summary>
        /// <param name="questionData">Question data object.</param>
        /// <returns>Combined description for rich card.</returns>
        public static string BuildCombinedDescriptionAsync(AdaptiveSubmitActionData questionData)
        {
            if (!string.IsNullOrWhiteSpace(questionData?.Subtitle?.Trim())
                || !string.IsNullOrWhiteSpace(questionData?.Title?.Trim())
                || !string.IsNullOrWhiteSpace(questionData?.ImageUrl?.Trim())
                || !string.IsNullOrWhiteSpace(questionData?.RedirectionUrl?.Trim()))
            {
                var answerModel = new AnswerModel
                {
                    Description = questionData?.Description.Trim(),
                    Title = questionData?.Title?.Trim(),
                    Subtitle = questionData?.Subtitle?.Trim(),
                    ImageUrl = questionData?.ImageUrl?.Trim(),
                    RedirectionUrl = questionData?.RedirectionUrl?.Trim(),
                };

                return JsonConvert.SerializeObject(answerModel);
            }
            else
            {
                return questionData.Description.Trim();
            }
        }
    }
}
