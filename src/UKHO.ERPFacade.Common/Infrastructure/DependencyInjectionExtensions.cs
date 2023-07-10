﻿using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using UKHO.ERPFacade.Common.Infrastructure.Authentication;
using UKHO.ERPFacade.Common.Infrastructure.Config;
using UKHO.ERPFacade.Common.Infrastructure.EventService;
using UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider;
using UKHO.ERPFacade.Common.Providers;

namespace UKHO.ERPFacade.Common.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddOptions<ErpPublishEventSource>().Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("ErpPublishEventSource").Bind(settings);
            });

            services.AddOptions<InteractiveLoginConfiguration>().Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("InteractiveLoginConfiguration").Bind(settings);
            });

            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

            services.AddSingleton<ICloudEventFactory, CloudEventFactory>()
                    .AddSingleton<IEventPublisher, EnterpriseEventServiceEventPublisher>()
            .AddSingleton<IAccessTokenCache, AccessTokenCache>()
            .AddLazyCache();

            services.AddSingleton<InteractiveTokenProvider>();
            services.AddSingleton<ManagedIdentityTokenProvider>();
            services.AddSingleton<ITokenProvider>((sp) =>
            {
                var useLocal = sp.GetRequiredService<IConfiguration>().GetValue<bool>("UseLocalResources");
                return useLocal ? sp.GetRequiredService<InteractiveTokenProvider>() : sp.GetRequiredService<ManagedIdentityTokenProvider>();
            });

            var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError().RetryAsync(3);

            services.AddHttpClient(EnterpriseEventServiceEventPublisher.EventServiceClientName, (sp, client) =>
            {
                ErpPublishEventSource config = sp.GetRequiredService<IOptions<ErpPublishEventSource>>().Value;
                var accessTokenCache = sp.GetRequiredService<IAccessTokenCache>();
                client.BaseAddress = new Uri(config.ServiceUrl);

                //string token = accessTokenCache.GetTokenAsync("1640eb84-077b-4795-b2ab-75d8f8bf0942/.default").Result;
                string token = accessTokenCache.GetTokenAsync($"{config.ClientId}/{config.PublisherScope}").Result;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            }).AddPolicyHandler(retryPolicy);

            return services;
        }
    }
}
