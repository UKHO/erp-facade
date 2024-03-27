using NUnit.Framework;
using UKHO.ERPFacade.Common.Providers;

namespace UKHO.ERPFacade.Common.UnitTests.Providers
{
    [TestFixture]
    public class WeekDetailsProviderTests
    {
        private WeekDetailsProvider _fakeWeekDetailsProvider;

        [SetUp]
        public void Setup()
        {
            _fakeWeekDetailsProvider = new WeekDetailsProvider();
        }

        [Test]
        [TestCase(2024, 3, false, "20240118")]
        [TestCase(2024, 3, true, "20240119")]
        public void GetDateOfWeekTest(int year, int week, bool correction, string expectedResult)
        {
            var result = _fakeWeekDetailsProvider.GetDateOfWeek(year, week, correction);

            Assert.That(expectedResult, Is.EqualTo(result));
        }
    }
}
