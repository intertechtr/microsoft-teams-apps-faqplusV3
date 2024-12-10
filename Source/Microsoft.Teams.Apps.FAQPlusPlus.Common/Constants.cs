﻿// <copyright file="Constants.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common
{
    /// <summary>
    /// constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Source.
        /// </summary>
        public const string Source = "Editorial";

        /// <summary>
        /// Regular expression pattern for valid redirection url.
        /// It checks whether the url is valid or not, while adding/editing the qna pair.
        /// </summary>
        public const string ValidRedirectUrlPattern = @"^(http|https|)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?([a-zA-Z0-9\-\?\,\'\/\+&%\$#_]+)";

        /// <summary>
        /// Name of the QnA metadata property to map with the date and time the item was added.
        /// </summary>
        public const string MetadataCreatedAt = "createdat";

        /// <summary>
        /// Name of the QnA metadata property to map with the user who created the item.
        /// </summary>
        public const string MetadataCreatedBy = "createdby";

        /// <summary>
        /// Name of the QnA metadata property to map with the conversation id of the item.
        /// </summary>
        public const string MetadataConversationId = "conversationid";

        /// <summary>
        ///   Name of the QnA metadata property to map with the date and time the item was updated.
        /// </summary>
        public const string MetadataUpdatedAt = "updatedat";

        /// <summary>
        /// Name of the QnA metadata property to map with the user who updated the item.
        /// </summary>
        public const string MetadataUpdatedBy = "updatedby";

        /// <summary>
        /// Name of the QnA metadata property to map with the activity reference id for future reference.
        /// </summary>
        public const string MetadataActivityReferenceId = "activityreferenceid";

        /// <summary>
        /// TakeAtour - text that triggers take a tour action for the user.
        /// </summary>
        public const string TakeATour = "take a tour";

        /// <summary>
        /// Feedback - text that renders share feedback card.
        /// </summary>
        public const string ShareFeedback = "share feedback";

        /// <summary>
        /// Text associated with share feedback command.
        /// </summary>
        public const string ShareFeedbackSubmitText = "ShareFeedback";

        /// <summary>
        /// Represents the command text to identify the action.
        /// </summary>
        public const string PreviewCardCommandText = "previewcard";
    }
}
