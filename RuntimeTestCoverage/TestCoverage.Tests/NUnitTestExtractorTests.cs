using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            Assert.That(fixture.AssemblyName, Is.EqualTo(assemblyName));
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
            Assert.That(testCases.Cases[0].MethodName, Is.EqualTo("TestSomething1"));
            Assert.That(testCases.Cases[0].Arguments.Length, Is.EqualTo(0));
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
            Assert.That(testMethods[0].Arguments[0], Is.EqualTo("true"));
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
            var testMethods = _sut.GetTestFixtureDetails(tree, _semanticModelMock).Cases;

            // assert
            Assert.That(testMethods.Count, Is.EqualTo(1));
            Assert.That(testMethods[0].MethodName, Is.EqualTo("TestSomething1"));
            Assert.That(testMethods[0].Arguments.Length, Is.EqualTo(1));
            Assert.That(testMethods[0].Arguments[0], Is.EqualTo("\"Test\""));
        }

        [TestCase("test", "\"test\"")]
        [TestCase(5, "5")]
        [TestCase(5.5, "5.5")]
        [TestCase(true, "true")]
        public void Shoul_Convert_SemanticConstantValue_To_CallableValue(object value, string expectedCallValue)
        {
            // arrange
            const string code = @"using Math.Data;
                                namespace Code{
                            public class Tests
                            {
	                            [TestCase(""Test"")]
	                            public void TestSomething1()
	                            {

	                            }	
                            }}";

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();
            _semanticModelMock.GetConstantValue(Arg.Any<SyntaxNode>()).Returns(value);

            // act
            var fixture = _sut.GetTestFixtureDetails(tree, _semanticModelMock);

            // assert
            Assert.That(fixture.Cases[0].Arguments[0], Is.EqualTo(expectedCallValue));
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

            var tree = CSharpSyntaxTree.ParseText(code).GetRoot().GetClassDeclarationSyntax();

            // act
            var testMethods = _sut.GetTestFixtureDetails(tree, _semanticModelMock).Cases;

            // assert
            Assert.That(testMethods.Count, Is.EqualTo(1));
            Assert.That(testMethods[0].MethodName, Is.EqualTo("TestSomething1"));
            Assert.That(testMethods[0].Arguments.Length, Is.EqualTo(1));
            Assert.That(testMethods[0].Arguments[0], Is.EqualTo("5+9"));
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
            Assert.That(testMethods[0].Arguments[0], Is.EqualTo("1"));
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
        public void ShouldExtract_NamespaceWithClassName_For_TestMethod()
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
            Assert.That(testCases.Namespace, Is.EqualTo("Code"));
            Assert.That(testCases.ClassName, Is.EqualTo("Tests"));
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
        public void ShouldNot_ExtractSetupMethodName_When_ItDoesNotExist()
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
            Assert.IsNull(testFixtureDetails.SetupMethodName);
        }

        [Test]
        public void Should_ExtractSetupMethodName_When_ItExists()
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
            Assert.That(testFixtureDetails.SetupMethodName, Is.EqualTo("AnyName"));
        }
    }
}
