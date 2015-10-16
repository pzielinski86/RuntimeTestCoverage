using NUnit.Framework;
using TestCoverage.CoverageCalculation;

namespace TestCoverage.Tests
{
    [TestFixture]
    public class TestFixtureDetailsTests
    {
        [Test]
        public void Should_OnlyCreateInstanceOfFixture_When_SetupMethodIsNotAvailable()
        {
            // arrange
            var sut=new TestFixtureDetails();
            const string expectedCode = "dynamic testInstance = new MathHelperTests();\r\n";
            sut.ClassName = "MathHelperTests";

            // act
            string code = sut.CreateSetupFixtureCode("testInstance");

            // assert
            Assert.That(code, Is.EqualTo(expectedCode));
        }

        [Test]
        public void Should_CallSetupMethod_When_ItExists()
        {
            // arrange
            var sut = new TestFixtureDetails();
            const string expectedCode = "testInstance.Init();";
            sut.ClassName = "MathHelperTests";
            sut.SetupMethodName = "Init";

            // act
            string code = sut.CreateSetupFixtureCode("testInstance");

            // assert
            Assert.That(code, Is.StringContaining(expectedCode));
        }
    }
}