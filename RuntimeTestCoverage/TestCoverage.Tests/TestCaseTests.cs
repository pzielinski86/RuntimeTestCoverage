using NUnit.Framework;
using TestCoverage.CoverageCalculation;

namespace TestCoverage.Tests
{
    [TestFixture]
    public class TestCaseTests
    {
        private TestCase _sut;

        [SetUp]
        public void Setup()
        {
            _sut=new TestCase(null);
        }

        [Test]
        public void Should_Call_TestWithoutParameters()
        {
            // arrange
            const string expectedCode = "testExecutor.Test();";

            _sut.MethodName = "Test";
            _sut.Arguments=new string[0];

            // act
            string code=_sut.CreateCallTestCode("testExecutor");

            // assert
            Assert.That(code,Is.EqualTo(expectedCode));
        }

        [Test]
        public void Should_Call_TestWithString()
        {
            // arrange
            const string expectedCode = "testExecutor.Test(\"Hello World!\");";

            _sut.MethodName = "Test";
            _sut.Arguments = new[] {"\"Hello World!\""};

            // act
            string code = _sut.CreateCallTestCode("testExecutor");

            // assert
            Assert.That(code, Is.EqualTo(expectedCode));
        }

        [Test]
        public void Should_Call_TestWithInteger()
        {
            // arrange
            const string expectedCode = "testExecutor.Test(1954);";

            _sut.MethodName = "Test";
            _sut.Arguments = new string[] {"1954" };

            // act
            string code = _sut.CreateCallTestCode("testExecutor");

            // assert
            Assert.That(code, Is.EqualTo(expectedCode));
        }

        [Test]
        public void Should_Call_TestWithNull()
        {
            // arrange
            const string expectedCode = "testExecutor.Test(null);";

            _sut.MethodName = "Test";
            _sut.Arguments = new string[] { null };

            // act
            string code = _sut.CreateCallTestCode("testExecutor");

            // assert
            Assert.That(code, Is.EqualTo(expectedCode));
        }

        [Test]
        public void Should_Call_TestWithBool()
        {
            // arrange
            const string expectedCode = "testExecutor.Test(false);";

            _sut.MethodName = "Test";
            _sut.Arguments = new string[] {"false" };

            // act
            string code = _sut.CreateCallTestCode("testExecutor");

            // assert
            Assert.That(code, Is.EqualTo(expectedCode));
        }

        [Test]
        public void Should_Call_TestWithFloat()
        {
            // arrange
            const string expectedCode = "testExecutor.Test(4141.1);";

            _sut.MethodName = "Test";
            _sut.Arguments = new string[] { "4141.1" };

            // act
            string code = _sut.CreateCallTestCode("testExecutor");

            // assert
            Assert.That(code, Is.EqualTo(expectedCode));
        }

        [Test]
        public void Should_Call_TestWithTwoParameters()
        {
            // arrange
            const string expectedCode = "testExecutor.Test(1, 2);";

            _sut.MethodName = "Test";
            _sut.Arguments = new string[] { "1","2"  };

            // act
            string code = _sut.CreateCallTestCode("testExecutor");

            // assert
            Assert.That(code, Is.EqualTo(expectedCode));
        }
    }
}