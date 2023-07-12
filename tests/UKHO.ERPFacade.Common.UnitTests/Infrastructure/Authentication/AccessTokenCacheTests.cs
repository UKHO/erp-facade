using System;
using System.Threading.Tasks;
using Azure.Core;
using FakeItEasy;
using FluentAssertions;
using LazyCache;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using UKHO.ERPFacade.Common.Infrastructure.Authentication;

namespace UKHO.ERPFacade.Common.UnitTests.Infrastructure.Authentication
{
    public class AccessTokenCacheTests
    {
        private const string Scope = "MyScope";
        private const string TokenValue = "MyToken";
        private ITokenProvider _fakeTokenProvider;
        private AccessTokenCache _fakeAccessTokenCache;

        [SetUp]
        public void Setup()
        {
            _fakeTokenProvider = A.Fake<ITokenProvider>();

            A.CallTo(() => _fakeTokenProvider.GetTokenAsync(Scope)).Returns(new AccessToken(TokenValue, DateTimeOffset.MaxValue));

            var cachingService = new CachingService();

            // we're fairly sure the cache has a shared/static cache between all instances
            // removing this causes the GetTokenAsync_UpdatesIfExpired test to fail when churned
            cachingService.Remove(Scope);
            _fakeAccessTokenCache = new AccessTokenCache(cachingService, _fakeTokenProvider);
        }

        [Test]
        public async Task WhenGetTokenAsyncIsCalled_ThenReturnsToken()
        {
            string result = await _fakeAccessTokenCache.GetTokenAsync(Scope);

            result.Should().Be(TokenValue);

            A.CallTo(() => _fakeTokenProvider.GetTokenAsync(Scope)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenGetTokenAsyncIsCalled_ThenUpdatesIfExpired()
        {
            A.CallTo(() => _fakeTokenProvider.GetTokenAsync(Scope)).ReturnsLazily(() => Task.FromResult(new AccessToken(TokenValue, DateTimeOffset.UtcNow.AddMilliseconds(200))));
            string result = await _fakeAccessTokenCache.GetTokenAsync(Scope);
            result.Should().NotBeNull();

            await Task.Delay(TimeSpan.FromMilliseconds(201));
            string result2 = await _fakeAccessTokenCache.GetTokenAsync(Scope);
            result2.Should().NotBeNull();
            A.CallTo(() => _fakeTokenProvider.GetTokenAsync(A<string>.Ignored)).MustHaveHappened();
        }

        [Test]
        public async Task WhenGetTokenAsyncIsCalled_ThenBlocksAttemptsToGetNewValueIfOtherValueIsRefreshing()
        {
            var counter = 0;

            DateTime endDate = DateTime.UtcNow.AddMilliseconds(500);

            A.CallTo(() => _fakeTokenProvider.GetTokenAsync(Scope)).ReturnsLazily(async () =>
            {
                counter++;
                await Task.Delay(300);
                return new AccessToken(TokenValue, DateTimeOffset.UtcNow.AddMilliseconds(200));
            });

            while (DateTime.UtcNow < endDate)
            {
                string _ = await _fakeAccessTokenCache.GetTokenAsync(Scope);
            }

            counter.Should().Be(1);
        }
    }
}
