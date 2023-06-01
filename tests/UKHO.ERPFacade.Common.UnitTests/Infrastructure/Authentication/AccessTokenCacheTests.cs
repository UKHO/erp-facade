using Azure.Core;
using FakeItEasy;
using LazyCache;
using NUnit.Framework;
using System.Threading.Tasks;
using System;
using UKHO.ERPFacade.Common.Infrastructure.Authentication;

namespace UKHO.ERPFacade.Common.UnitTests.Infrastructure.Authentication
{
    internal class AccessTokenCacheTests
    {
        private const string Scope = "MyScope";
        private const string TokenValue = "MyToken";
        private ITokenProvider _fakeTokenProvider;
        private AccessTokenCache _tokenCache;

        [SetUp]
        public void Setup()
        {
            _fakeTokenProvider = A.Fake<ITokenProvider>();

            A.CallTo(() => _fakeTokenProvider.GetTokenAsync(Scope)).Returns(new AccessToken(TokenValue, DateTimeOffset.MaxValue));

            var cachingService = new CachingService();

            // we're fairly sure the cache has a shared/static cache between all instances
            // removing this causes the GetTokenAsync_UpdatesIfExpired test to fail when churned
            cachingService.Remove(Scope);
            _tokenCache = new AccessTokenCache(cachingService, _fakeTokenProvider);
        }

        [Test]
        public async Task GetTokenAsync_ReturnsToken()
        {
            string result = await _tokenCache.GetTokenAsync(Scope);
            string result2 = await _tokenCache.GetTokenAsync(Scope);

            Assert.AreEqual(TokenValue, result);
            Assert.AreEqual(TokenValue, result2);
            A.CallTo(() => _fakeTokenProvider.GetTokenAsync(Scope)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task GetTokenAsync_UpdatesIfExpired()
        {
            A.CallTo(() => _fakeTokenProvider.GetTokenAsync(Scope)).ReturnsLazily(() => Task.FromResult(new AccessToken(TokenValue, DateTimeOffset.UtcNow.AddMilliseconds(200))));
            string result = await _tokenCache.GetTokenAsync(Scope);
            Assert.IsNotNull(result);

            await Task.Delay(TimeSpan.FromMilliseconds(201));
            string result2 = await _tokenCache.GetTokenAsync(Scope);
            Assert.IsNotNull(result2);
            A.CallTo(() => _fakeTokenProvider.GetTokenAsync(A<string>.Ignored)).MustHaveHappenedTwiceExactly();
        }

        [Test]
        public async Task GetTokenAsync_BlocksAttemptsToGetNewValueIfOtherValueIsRefreshing()
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
                string _ = await _tokenCache.GetTokenAsync(Scope);
            }

            Assert.AreEqual(1, counter);
        }
    }
}