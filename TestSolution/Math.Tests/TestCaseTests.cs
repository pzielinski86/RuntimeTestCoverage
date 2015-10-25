using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Math.Contracts;
using Math;

namespace Math.Tests
{
    [TestFixture]
    public class TestCaseTests
    {
        private TestCase _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new TestCase(null);
        }



        

        [TestCase(Contracts.PersonType.B)]
        public void Should_Call_TestWithoutParameters(PersonType b)
        {
            // arrange
            const string expectedCode = "testExecutor.Test();";

            _sut.MethodName = "Test";
            _sut.Arguments = new object[0];

            // act
            string code = _sut.CreateCallTestCode("testExecutor");

            // assert
            Assert.That(code, Is.EqualTo(expectedCode));

            Assert.That(b,Is.EqualTo(PersonType.A));
        }

        [TestCase("test") ]
        public void Should_Call_TestWithString(string var)
        {
            // arrange
            const string expectedCode = "testExecutor.Test(\"Hello World!\");";

            _sut.MethodName = "Test";
            _sut.Arguments = new object[] { "Hello World!" };

            // act
            string code = _sut.CreateCallTestCode("testExecutor");

            // assert
            Assert.That(code, Is.EqualTo(expectedCode));
            Assert.That(var, Is.EqualTo("test"));
        }

        [Test]
        public void Should_Call_TestWithInteger()
        {
            // arrange
            const string expectedCode = "testExecutor.Test(1954);";

            _sut.MethodName = "Test";
            _sut.Arguments = new object[] { 1954 };

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
            _sut.Arguments = new object[] { false };

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
            _sut.Arguments = new object[] { 4141.1 };

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
            _sut.Arguments = new object[] { 1, 2 };

            // act
            string code = _sut.CreateCallTestCode("testExecutor");

            // assert
            Assert.That(code, Is.EqualTo(expectedCode));
        }
    }
}
