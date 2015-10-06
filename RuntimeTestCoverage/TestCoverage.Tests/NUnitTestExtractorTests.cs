using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using TestCoverage.CoverageCalculation;

namespace TestCoverage.Tests
{
    [TestFixture]
    public class NUnitTestExtractorTests
    {
        private NUnitTestExtractor _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new NUnitTestExtractor();
        }

        [Test]
        public void ShouldExtract_PublicTests()
        {
            // arrange
            const string code = @"public class Tests
                        {
	                        [Test]
	                        public void TestSomething1()
	                        {

	                        }
                        }";

            var tree = CSharpSyntaxTree.ParseText(code);

            // act
            var testMethods = _sut.GetTestCases(tree.GetRoot());

            // assert
            Assert.That(testMethods.Length,Is.EqualTo(1));
            Assert.IsInstanceOf<MethodDeclarationSyntax>(testMethods[0]);

            Assert.That(testMethods[0].MethodName,Is.EqualTo("TestSomething1"));
            Assert.That(testMethods[0].Arguments.Length,Is.EqualTo(0));
        }

        [Test]
        public void ShouldExtract_PrivateTests()
        {
            // arrange
            const string code = @"public class Tests
                        {
	                        [Test]
	                        private void TestSomething1()
	                        {

	                        }	
                        }";

            var tree = CSharpSyntaxTree.ParseText(code);

            // act
            var testMethods = _sut.GetTestCases(tree.GetRoot());

            // assert
            Assert.That(testMethods.Length, Is.EqualTo(1));
            Assert.IsInstanceOf<MethodDeclarationSyntax>(testMethods[0]);

            Assert.That(testMethods[0].MethodName, Is.EqualTo("TestSomething1"));
        }

        [Test]
        public void ShouldNot_ExtractHelperMethods()
        {
            // arrange
            const string code = @"public class Tests
                        {
	                        private void NotATest()
	                        {

	                        }	
                        }";

            var tree = CSharpSyntaxTree.ParseText(code);

            // act
            var testMethods = _sut.GetTestCases(tree.GetRoot());

            // assert
            Assert.That(testMethods.Length, Is.EqualTo(0));
        }

        [Test]
        public void Should_ExtractPublicTestClasses()
        {
            // arrange
            const string code = @"[TestFixture]public class Tests
                        {	               
                        }";

            var tree = CSharpSyntaxTree.ParseText(code);

            // act
            var testClasses = _sut.GetTestClasses(tree.GetRoot());

            // assert
            Assert.That(testClasses.Length, Is.EqualTo(1));
            Assert.IsInstanceOf<ClassDeclarationSyntax>(testClasses[0]);
        }


        [Test]
        public void ShouldNot_ExtractClassesWhichAreNotTestFixtures()
        {
            // arrange
            const string code = @"public class Tests
                        {	               
                        }";

            var tree = CSharpSyntaxTree.ParseText(code);

            // act
            var testClasses = _sut.GetTestClasses(tree.GetRoot());

            // assert
            Assert.That(testClasses.Length, Is.EqualTo(0));
        }
    }
}
