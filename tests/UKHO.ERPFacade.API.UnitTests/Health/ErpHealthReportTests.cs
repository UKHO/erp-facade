using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NUnit.Framework;
using UKHO.ERPFacade.API.Health;

namespace UKHO.ERPFacade.API.UnitTests.Health
{
    [TestFixture]
    public class ErpHealthReportTests
    {
        [Test]
        public void WhenConstructorIsCalled_ThenInitializeProperties()
        {
            // Arrange
            var entries = new Dictionary<string, ErpHealthReportEntry>();
            var totalDuration = TimeSpan.FromSeconds(5);

            // Act
            var report = new ErpHealthReport(entries, totalDuration);

            // Assert
            report.Entries.Should().BeSameAs(entries);
            report.TotalDuration.Should().Be(totalDuration);
        }

        [Test]
        public void WhenValidInputsPassed_ThenErpHealthReportShouldBeCreated()
        {
            // Arrange
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
            var result = ErpHealthReport.CreateFrom(healthReport);

            // Assert
            result.Status.Should().Be(ErpHealthStatus.Healthy);
            result.TotalDuration.Should().Be(TimeSpan.FromSeconds(5));
            result.Entries.Count.Should().Be(1);
            result.Entries["TestEntry"].Description.Should().Be("Test description");
        }

        [Test]
        public void WhenInCaseOfException_ThenCreateUnhealthyReport()
        {
            // Arrange
            var exception = new Exception("Test exception");
            var healthReportEntries = new Dictionary<string, HealthReportEntry>
            {
                {
                    "TestEntry", new HealthReportEntry(
                        status: HealthStatus.Unhealthy,
                        description: null,
                        duration: TimeSpan.FromSeconds(1),
                        exception: exception,
                        data: new Dictionary<string, object>(),
                        tags: new List<string>())
                }
            };
            var healthReport = new HealthReport(healthReportEntries, TimeSpan.FromSeconds(5));

            // Act
            var result = ErpHealthReport.CreateFrom(healthReport);

            // Assert
            ErpHealthStatus.Unhealthy.Should().Be(ErpHealthStatus.Unhealthy);
            result.TotalDuration.Should().Be(TimeSpan.FromSeconds(5));
            result.Entries.Count.Should().Be(1);
            result.Entries["TestEntry"].Exception.Should().Be("Test exception");
            result.Entries["TestEntry"].Description.Should().Be("Test exception");
        }

        [Test]
        public void WhenInCaseOfException_ThenCreateUnhealthyReportWithCustomExceptionMessage()
        {
            // Arrange
            var exception = new Exception("Test exception");
            var healthReportEntries = new Dictionary<string, HealthReportEntry>
            {
                {
                    "TestEntry", new HealthReportEntry(
                        status: HealthStatus.Unhealthy,
                        description: null,
                        duration: TimeSpan.FromSeconds(1),
                        exception: exception,
                        data: new Dictionary<string, object>(),
                        tags: new List<string>())
                }
            };
            var healthReport = new HealthReport(healthReportEntries, TimeSpan.FromSeconds(5));
            Func<Exception, string> customExceptionMessage = ex => "Custom message: " + ex.Message;

            // Act
            var result = ErpHealthReport.CreateFrom(healthReport, customExceptionMessage);

            // Assert
            ErpHealthStatus.Unhealthy.Should().Be(ErpHealthStatus.Unhealthy);
            result.TotalDuration.Should().Be(TimeSpan.FromSeconds(5));
            result.Entries.Count.Should().Be(1);
            result.Entries["TestEntry"].Exception.Should().Be("Custom message: Test exception");
            result.Entries["TestEntry"].Description.Should().Be("Custom message: Test exception");
        }

        [Test]
        public void WhenInCaseOfException_ThenCreateUnhealthyReportWithCustomEntryName()
        {
            // Arrange
            var exception = new Exception("Test exception");
            var entryName = "CustomEntry";

            // Act
            var result = ErpHealthReport.CreateFrom(exception, entryName);

            // Assert
            ErpHealthStatus.Unhealthy.Should().Be(ErpHealthStatus.Unhealthy);
            result.TotalDuration.Should().Be(TimeSpan.FromSeconds(0));
            result.Entries.Count.Should().Be(1);
            result.Entries[entryName].Exception.Should().Be("Test exception");
            result.Entries[entryName].Description.Should().Be("Test exception");
        }
    }
}
