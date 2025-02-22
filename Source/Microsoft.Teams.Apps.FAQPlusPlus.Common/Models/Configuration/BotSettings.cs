﻿// <copyright file="BotSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration
{
    /// <summary>
   /// Provides app settings related to FaqPlusPlus bot.
   /// </summary>
    public class BotSettings
    {
        /// <summary>
        /// Gets or sets access cache expiry in days.
        /// </summary>
        public int AccessCacheExpiryInDays { get; set; }

        /// <summary>
        /// Gets or sets application base uri.
        /// </summary>
        public string AppBaseUri { get; set; }

        /// <summary>
        /// Gets or sets user app id.
        /// </summary>
        public string UserAppId { get; set; }

        /// <summary>
        /// Gets or sets user app password.
        /// </summary>
        public string UserAppPassword { get; set; }

        /// <summary>
        /// Gets or sets access tenant id string.
        /// </summary>
        public string TenantId { get; set; }
        public string AOAI_ENDPOINT { get; set; }
        public string AOAI_KEY { get; set; }
        public string AOAI_DEPLOYMENTID { get; set; }
        public string SEARCH_INDEX_NAME { get; set; }
        public string SEARCH_SERVICE_NAME { get; set; }
        public string SEARCH_QUERY_KEY { get; set; }
        public string SettingForPrompt { get; set; }
        public string SettingForTemperature { get; set; }
        public string SettingForMaxToken{ get; set; }
        public string SettingForTopK{ get; set; }
        public string AOAI_EmbeddingModelName { get; set; }
    }
}
