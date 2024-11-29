using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NUnit.Framework;
using UKHO.ERPFacade.API.Health;

namespace UKHO.ERPFacade.API.UnitTests.Health
{
    [TestFixture]
    public class HealthResponseWriterTests
    {
        [Test]
        public async Task WhenValidInputsArePassed_ThenCreateHealthyReport()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var responseStream = new MemoryStream();
            httpContext.Response.Body = responseStream;

            var healthReportEntries = new Dictionary<string, HealthReportEntry>
            {
                {
                    "TestEntry", new HealthReportEntry(
                        status: HealthStatus.Healthy,
                        description: "Test description",
                        duration: TimeSpan.FromSeconds(1),
                        exception: null,
                        data: new Dictionary<string, object>(),
                        tags: new List<string>())
                }
            };
            var healthReport = new HealthReport(healthReportEntries, TimeSpan.FromSeconds(5));

            // Act
            await HealthResponseWriter.WriteHealthCheckUiResponse(httpContext, healthReport);

            // Assert
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
            responseBody.Contains("status\": \"Healthy\"").Should().BeTrue();
        }

        [Test]
        public async Task WhenNullInputsArePassed_ThenCreateEmptyReport()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var responseStream = new MemoryStream();
            httpContext.Response.Body = responseStream;

            // Act
            await HealthResponseWriter.WriteHealthCheckUiResponse(httpContext, null);

            // Assert
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
            responseBody.Should().Be("{}");
        }
    }
}
