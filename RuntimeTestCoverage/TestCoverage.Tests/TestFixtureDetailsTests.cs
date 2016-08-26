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
            var sut = new TestFixtureDetails();
            const string expectedCode = "Type testFixtureType = Type.GetType(\"Tests.MathHelperTests,Tests, Version=1.0.0.0\");\r\n" +
                                         "object testInstance = System.Activator.CreateInstance(testFixtureType);\r\n";
            sut.FullClassName = "Tests.MathHelperTests";
            sut.AssemblyName = "Tests, Version=1.0.0.0";

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
            const string expectedCode = "testFixtureType.GetMethod(\"Init\",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Invoke(testInstance, null);";
            sut.FullClassName = "Tests.MathHelperTests";
            sut.TestSetUpMethodName = "Init";

            // act
            string code = sut.CreateSetupFixtureCode("testInstance");

            // assert
            Assert.That(code, Is.StringContaining(expectedCode));
        }
    }
}