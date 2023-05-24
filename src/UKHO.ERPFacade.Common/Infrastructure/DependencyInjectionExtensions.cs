using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly.Extensions.Http;
using Polly;
using System.Net.Http.Headers;
using UKHO.ERPFacade.Common.Infrastructure.Authentication;
using UKHO.ERPFacade.Common.Infrastructure.Config;
using UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider;
using UKHO.ERPFacade.Common.Infrastructure.EventService;
using UKHO.ERPFacade.Common.Providers;

namespace UKHO.ERPFacade.Common.Infrastructure
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddOptions<NotificationsConfiguration>().Configure<IConfiguration>((settings, configuration) => { configuration.Bind(settings); });
            services.AddOptions<EnterpriseEventServiceConfiguration>().Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("EnterpriseEventServiceConfiguration").Bind(settings);
            });

            services.AddOptions<InteractiveLoginConfiguration>().Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("InteractiveLoginConfiguration").Bind(settings);
            });

            services.AddSingleton<IDateTimeProvider, DateTimeProvider>()
                    .AddSingleton<IUniqueIdentifierFactory, UniqueIdentifierFactory>();

            services.AddSingleton<ICloudEventFactory, CloudEventFactory>()
                    .AddSingleton<IEventPublisher, EnterpriseEventServiceEventPublisher>();
            //.AddSingleton<IAccessTokenCache, AccessTokenCache>()
            //.AddLazyCache();

            services.AddSingleton<InteractiveTokenProvider>();
            services.AddSingleton<ManagedIdentityTokenProvider>();
            services.AddSingleton<ITokenProvider>((sp) =>
            {
                var useLocal = sp.GetRequiredService<IConfiguration>().GetValue<bool>("UseLocalResources");
                if (useLocal)
                {
                    return sp.GetRequiredService<InteractiveTokenProvider>();
                }

                return sp.GetRequiredService<ManagedIdentityTokenProvider>();
            });

            var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError().RetryAsync(3);

            services.AddHttpClient(EnterpriseEventServiceEventPublisher.EventServiceClientName, (sp, client) =>
            {
                EnterpriseEventServiceConfiguration config = sp.GetRequiredService<IOptions<EnterpriseEventServiceConfiguration>>().Value;
                var accessTokenCache = sp.GetRequiredService<IAccessTokenCache>();
                client.BaseAddress = new Uri(config.ServiceUrl);
                string token = accessTokenCache.GetTokenAsync($"{config.ClientId}/{config.PublisherScope}").Result;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }).AddPolicyHandler(retryPolicy);

            return services;
        }
    }
}