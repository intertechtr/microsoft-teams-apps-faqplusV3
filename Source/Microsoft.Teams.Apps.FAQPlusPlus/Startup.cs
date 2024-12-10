// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Teams.Apps.FAQPlusPlus.Bots;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Credentials;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Components;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Extensions;
    using global::Azure;

    /// <summary>
    /// This a Startup class for this Bot.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">Startup Configuration.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
    }

        /// <summary>
        /// Gets Configurations Interfaces.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Application Builder.</param>
        /// <param name="env">Hosting Environment.</param>
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRequestLocalization();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller}/{action=Index}/{id?}");
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"> Service Collection Interface.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddApplicationInsightsTelemetry();

            services.Configure<BotSettings>(botSettings =>
            {
                botSettings.AppBaseUri = this.Configuration["AppBaseUri"];
                botSettings.UserAppId = this.Configuration["UserAppId"];
                botSettings.UserAppPassword = this.Configuration["UserAppPassword"];
                botSettings.TenantId = this.Configuration["TenantId"];
                botSettings.AOAI_ENDPOINT = this.Configuration["AOAI_ENDPOINT"];
                botSettings.AOAI_KEY = this.Configuration["AOAI_KEY"];
                botSettings.AOAI_DEPLOYMENTID = this.Configuration["AOAI_DEPLOYMENTID"];
                botSettings.SEARCH_INDEX_NAME = this.Configuration["SEARCH_INDEX_NAME"];
                botSettings.SEARCH_SERVICE_NAME = this.Configuration["SEARCH_SERVICE_NAME"];
                botSettings.SEARCH_QUERY_KEY = this.Configuration["SEARCH_QUERY_KEY"];
                botSettings.SettingForPrompt = this.Configuration["SettingForPrompt"];
                botSettings.SettingForTemperature = this.Configuration["SettingForTemperature"];
                botSettings.SettingForMaxToken = this.Configuration["SettingForMaxToken"];
                botSettings.SettingForTopK = this.Configuration["SettingForTopK"];
                botSettings.AOAI_EmbeddingModelName = this.Configuration["AOAI_EmbeddingModelName"];
            });

            services.AddHttpClient();
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            services.AddSingleton<IBotFrameworkHttpAdapter, BotFrameworkHttpAdapter>();
            services.AddSingleton<UserAppCredentials>();

            services.AddSingleton<IMemoryCache, MemoryCache>();
            services.AddTransient(sp => (BotFrameworkAdapter)sp.GetRequiredService<IBotFrameworkHttpAdapter>());
            services.AddTransient<FaqPlusUserBot>();
            services.AddTransient<TurnContextExtension>();
            ComponentsRegistery.AddComponentServices(services);

            // Create the telemetry middleware(used by the telemetry initializer) to track conversation events
            services.AddSingleton<TelemetryLoggerMiddleware>();
            services.AddMemoryCache();

            // Add i18n.
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var defaultCulture = CultureInfo.GetCultureInfo(this.Configuration["i18n:DefaultCulture"]);
                var supportedCultures = this.Configuration["i18n:SupportedCultures"].Split(',')
                    .Select(culture => CultureInfo.GetCultureInfo(culture))
                    .ToList();

                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new BotLocalizationCultureProvider(),
                };
            });
        }
    }
}
