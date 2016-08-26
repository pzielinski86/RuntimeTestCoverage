using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NSubstitute;
using NUnit.Framework;
using TestCoverage.Compilation;
using TestCoverage.CoverageCalculation;
using TestCoverage.Extensions;

namespace TestCoverage.Tests
{

    [TestFixture]
    public class NUnitTestExtractorTests
    {
        private NUnitTestExtractor _sut;
        private ISemanticModel _semanticModelMock;

        [SetUp]
        public void Setup()
        {
            _sut = new NUnitTestExtractor();
            _semanticModelMock = Substitute.For<ISemanticModel>();
            _semanticModelMock.GetConstantValue(Arg.Any<SyntaxNode>()).Returns((string)null);
        }

        [Test]
        public void ShouldExtract_AssemblyName()
        {
            // arrange
            const string assemblyName = "assembly name";
            _semanticModelMock.GetAssemblyName().Returns(assemblyName);

            const string code = @"namespace Code{
                            public class Tests
                            {
	                            [Test]
	                            public void TestSomething1()
	                            {

	                            }	
                            }}";

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act
            var fixture = _sut.GetTestFixtureDetails(tree, _semanticModelMock);

            // assert
            Assert.That(fixture.AssemblyName, Is.EqualTo("assembly name_COVERAGE.dll"));
        }

        [Test]
        public void ShouldExtract_PublicTests()
        {
            // arrange
            const string code = @"namespace Code{
                            public class Tests
                            {
	                            [Test]
	                            public void TestSomething1()
	                            {

	                            }	
                            }}";

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act
            var testCases = _sut.GetTestFixtureDetails(tree, _semanticModelMock);

            // assert
            Assert.That(testCases.Cases.Count, Is.EqualTo(1));
            Assert.IsFalse(testCases.Cases[0].IsAsync);
            Assert.That(testCases.Cases[0].MethodName, Is.EqualTo("TestSomething1"));
            Assert.That(testCases.Cases[0].Arguments.Length, Is.EqualTo(0));
        }
        
        [Test]
        public void ShouldIgnore_AsyncVoid_Tests()
        {
            // arrange
            const string code = @"namespace Code{
                            public class Tests
                            {
	                            [Test]
	                            public async void TestSomething1()
	                            {

	                            }	
                            }}";

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act  
            var testCases = _sut.GetTestFixtureDetails(tree, _semanticModelMock);

            // assert
            Assert.That(testCases.Cases.Count, Is.EqualTo(0));
        }

        [Test]
        public void ShouldNotIgnore_AsyncReturningTask_Tests()
        {
            // arrange
            const string code = @"namespace Code{
                            public class Tests
                            {
	                            [Test]
	                            public async Task TestSomething1()
	                            {

	                            }	
                            }}";

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act  
            var testCases = _sut.GetTestFixtureDetails(tree, _semanticModelMock);

            // assert
            Assert.That(testCases.Cases.Count, Is.EqualTo(1));
            Assert.IsTrue(testCases.Cases[0].IsAsync);
        }

        [Test]
        public void ShouldIgnore_AsyncVoid_TestCases()
        {
            // arrange
            const string code = @"namespace Code{
                            public class Tests
                            {
	                            [TestCase(true)]
	                            public async void TestSomething1(bool g)
	                            {

	                            }	
                            }}";

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act  
            var testCases = _sut.GetTestFixtureDetails(tree, _semanticModelMock);

            // assert
            Assert.That(testCases.Cases.Count, Is.EqualTo(0));
        }

        [Test]
        public void ShouldNotIgnore_AsyncReturningTask_TestCases()
        {
            // arrange
            const string code = @"namespace Code{
                            public class Tests
                            {
	                            [TestCase(true)]
	                            public async Task TestSomething1(bool a)
	                            {

	                            }	
                            }}";

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act  
            var testCases = _sut.GetTestFixtureDetails(tree, _semanticModelMock);

            // assert
            Assert.That(testCases.Cases.Count, Is.EqualTo(1));
            Assert.IsTrue(testCases.Cases[0].IsAsync);
        }

        [Test]
        public void ShouldExtract_TestCase_With_Bool_Parameter()
        {
            // arrange
            const string code = @"namespace Code{
                            public class Tests
                            {
	                            [TestCase(true)]
	                            public void TestSomething1()
	                            {

	                            }	
                            }}";

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act
            var testMethods = _sut.GetTestFixtureDetails(tree, _semanticModelMock).Cases;

            // assert
            Assert.That(testMethods.Count, Is.EqualTo(1));
            Assert.That(testMethods[0].MethodName, Is.EqualTo("TestSomething1"));
            Assert.That(testMethods[0].Arguments.Length, Is.EqualTo(1));
            Assert.That(testMethods[0].Arguments[0], Is.EqualTo(true));
        }

        [Test]
        public void ShouldExtract_TestCase_With_String_Parameter()
        {
            // arrange
            const string code = @"namespace Code{
                            public class Tests
                            {
	                            [TestCase(""Test"")]
	                            public void TestSomething1()
	                            {

	                            }	
                            }}";

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act
            var testCases = _sut.GetTestFixtureDetails(tree, _semanticModelMock).Cases;

            // assert
            Assert.That(testCases.Count, Is.EqualTo(1));
            Assert.IsFalse(testCases[0].IsAsync);
            Assert.That(testCases[0].MethodName, Is.EqualTo("TestSomething1"));
            Assert.That(testCases[0].Arguments.Length, Is.EqualTo(1));
            Assert.That(testCases[0].Arguments[0], Is.EqualTo("Test"));
        }   

        [Test]
        public void ShouldExtract_TestCase_With_Integer_Expression_Parameter()
        {
            // arrange
            const string code = @"namespace Code{
                            public class Tests
                            {
	                            [TestCase(5+9)]
	                            public void TestSomething1()
	                            {

	                            }	
                            }}";

            _semanticModelMock.GetConstantValue(Arg.Any<SyntaxNode>()).Returns(14);
            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act
            var testMethods = _sut.GetTestFixtureDetails(tree, _semanticModelMock).Cases;

            // assert
            Assert.That(testMethods.Count, Is.EqualTo(1));
            Assert.That(testMethods[0].MethodName, Is.EqualTo("TestSomething1"));
            Assert.That(testMethods[0].Arguments.Length, Is.EqualTo(1));
            Assert.That(testMethods[0].Arguments[0], Is.EqualTo(14));
        }

        [Test]
        public void ShouldExtract_TestCase_With_PositiveIntegerParameter()
        {
            // arrange
            const string code = @"namespace Code{
                            public class Tests
                            {
	                            [TestCase(1]
	                            public void TestSomething1()
	                            {

	                            }	
                            }}";

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act
            var testMethods = _sut.GetTestFixtureDetails(tree, _semanticModelMock).Cases;

            // assert
            Assert.That(testMethods.Count, Is.EqualTo(1));
            Assert.That(testMethods[0].MethodName, Is.EqualTo("TestSomething1"));
            Assert.That(testMethods[0].Arguments.Length, Is.EqualTo(1));
            Assert.That(testMethods[0].Arguments[0], Is.EqualTo(1));
        }   

        [Test]
        public void ShouldExtract_OnlyTestCase_When_Test_And_TestCase_AraAvailable()
        {
            // arrange
            const string code = @"namespace Code{
                            public class Tests
                            {
                                [Test]
	                            [TestCase(1,""Test"",true)]
	                            public void TestSomething1()
	                            {

	                            }	
                            }}";

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act
            var testMethods = _sut.GetTestFixtureDetails(tree, _semanticModelMock).Cases;

            // assert
            Assert.That(testMethods.Count, Is.EqualTo(1));
            Assert.That(testMethods[0].Arguments.Length, Is.EqualTo(3));
        }

        [Test]
        public void ShouldExtract_TestCase_And_Test_FromTestFixture()
        {
            // arrange
            const string code = @"namespace Code{
                            public class Tests
                            {
                                [Test]
	                            [TestCase(1,""Test"",true)]
	                            public void TestSomething1()
	                            {

	                            }	

                                [Test]
	                            public void TestSomething2()
	                            {

	                            }	
                            }}";

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act
            var testMethods = _sut.GetTestFixtureDetails(tree, _semanticModelMock).Cases;

            // assert
            Assert.That(testMethods.Count, Is.EqualTo(2));
            Assert.That(testMethods[0].Arguments.Length, Is.EqualTo(3));
            Assert.That(testMethods[1].Arguments.Length, Is.EqualTo(0));
        }

        [Test]
        public void ShouldExtract_FullClassName_For_TestMethod()
        {
            // arrange
            const string code = @"namespace Code{
                            public class Tests
                            {
	                            [Test]
	                            public void TestSomething1()
	                            {

	                            }	
                            }}";

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            _semanticModelMock.GetFullName(tree).Returns("Tests.Code");

            // act
            var testCases = _sut.GetTestFixtureDetails(tree, _semanticModelMock);

            // assert
            Assert.That(testCases.Cases.Count, Is.EqualTo(1));
            Assert.That(testCases.FullClassName, Is.EqualTo("Tests.Code"));
        }

        [Test]
        public void ShouldExtract_PrivateTests()
        {
            // arrange
            const string code = @"namespace Code{
                            public class Tests
                            {
	                            [Test]
	                            private void TestSomething1()
	                            {

	                            }	
                            }}";

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act
            var testMethods = _sut.GetTestFixtureDetails(tree, _semanticModelMock).Cases;

            // assert
            Assert.That(testMethods.Count, Is.EqualTo(1));
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

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act
            var testMethods = _sut.GetTestFixtureDetails(tree, _semanticModelMock).Cases;

            // assert
            Assert.That(testMethods.Count, Is.EqualTo(0));
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

        [Test]
        public void ShouldNot_ExtractSetUpMethodName_When_ItDoesNotExist()
        {
            // arrange
            const string code = @"public class Tests
                        {
	                        private void NotATest()
	                        {

	                        }	
                        }";

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act
            var testFixtureDetails = _sut.GetTestFixtureDetails(tree, _semanticModelMock);

            // assert
            Assert.IsNull(testFixtureDetails.TestSetUpMethodName);
        }

        [Test]
        public void Should_ExtractTestSetUpMethodName_When_ItExists()
        {
            // arrange
            const string code = @"public class Tests
                        {
                            [SetUp]
	                        private void AnyName()
	                        {

	                        }	
                        }";

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act
            var testFixtureDetails = _sut.GetTestFixtureDetails(tree, _semanticModelMock);

            // assert
            Assert.That(testFixtureDetails.TestSetUpMethodName, Is.EqualTo("AnyName"));
        }

        [Test]
        public void Should_ExtractTestTearDownMethodName_When_ItExists()
        {
            // arrange
            const string code = @"public class Tests
                        {
                            [TearDown]
	                        private void AnyName()
	                        {

	                        }	
                        }";

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act
            var testFixtureDetails = _sut.GetTestFixtureDetails(tree, _semanticModelMock);

            // assert
            Assert.That(testFixtureDetails.TestTearDownMethodName, Is.EqualTo("AnyName"));
        }

        [Test]
        public void Should_ExtractFixtureSetUpMethodName_When_ItExists()
        {
            // arrange
            const string code = @"public class Tests
                        {
                            [TestFixtureSetUp]
	                        private void AnyName()
	                        {

	                        }	
                        }";

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act
            var testFixtureDetails = _sut.GetTestFixtureDetails(tree, _semanticModelMock);

            // assert
            Assert.That(testFixtureDetails.TestFixtureSetUpMethodName, Is.EqualTo("AnyName"));
        }

        [Test]
        public void Should_ExtractFixtureTearDownMethodName_When_ItExists()
        {
            // arrange
            const string code = @"public class Tests
                        {
                            [TestFixtureTearDown]
	                        private void AnyName()
	                        {

	                        }	
                        }";

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act
            var testFixtureDetails = _sut.GetTestFixtureDetails(tree, _semanticModelMock);

            // assert
            Assert.That(testFixtureDetails.TestFixtureTearDownMethodName, Is.EqualTo("AnyName"));
        }
    }
}
