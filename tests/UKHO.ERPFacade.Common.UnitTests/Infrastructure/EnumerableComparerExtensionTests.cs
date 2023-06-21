using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using UKHO.ERPFacade.Common.Infrastructure;

namespace UKHO.ERPFacade.Common.UnitTests.Infrastructure
{
    public class EnumerableComparerExtensionTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void WhenBothListsAreEmpty_ThenReturnsTrue()
        {
            var left = new List<int>();
            var right = new List<int>();

            left.AreEquivalent(right).Should().BeTrue();
        }

        [Test]
        public void WhenBothListsAreEqual_ThenReturnsTrue()
        {
            var left = new List<int> { 1, 2, 3 };
            var right = new List<int> { 1, 2, 3 };

            left.AreEquivalent(right).Should().BeTrue();
        }

        [Test]
        public void WhenBothListsAreNotEqual_ThenReturnsFalse()
        {
            var left = new List<int> { 1, 2 };
            var right = new List<int> { 1, 2, 3 };

            left.AreEquivalent(right).Should().BeFalse();
        }
    }
}
