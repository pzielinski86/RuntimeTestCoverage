using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using TestCoverage.Extensions;

namespace TestCoverage.Tests.Extensions
{
    [TestFixture]
    public class SyntaxNodeExtensions
    {
        [Test]
        public void GetMethodAt_Should_Return_Method_When_PositionIs_InsideMethod()
        {
            // arrange
            const string code = "class Sample" +
                          "{ " +
                              "public void Test() " +
                              "{ " +
                                 "int a=0;" +
                              "}" +
                          "}";

            var tree = CSharpSyntaxTree.ParseText(code);
            int position = code.IndexOf("int a");

            // act
            MethodDeclarationSyntax method = tree.GetRoot().GetMethodAt(position);

            // assert
            Assert.IsNotNull(method);
            Assert.That(method.Identifier.ToString(), Is.EqualTo("Test"));
        }

        [Test]
        public void GetMethodAt_Should_Return_Null_When_PositionIs_OutsideMethods()
        {
            // arrange
            const string code = "class Sample" +
                          "{ " +
                              "public void Test() " +
                              "{ " +
                                 "int a=0;" +
                              "}" +
                          "}";

            var tree = CSharpSyntaxTree.ParseText(code);
            int position = code.IndexOf("}")+1;

            // act
            MethodDeclarationSyntax method = tree.GetRoot().GetMethodAt(position);

            // assert
            Assert.IsNull(method);
        }

        [Test]
        public void GetMethodAt_Should_Return_Method_When_PositionIs_InsideMethod_And_Method_Is_InSurroundedByOtherMethods()
        {
            // arrange
            const string code = "class Sample" +
                          "{ " +
                              "public void Test1() " +
                              "{ " +
                                 "int a=0;" +
                              "}" +
                              
                              "public void Test2() " +
                              "{ " +
                                 "int b=0;" +
                              "}" +
                             
                              "public void Test3() " +
                              "{ " +
                                 "int c=0;" +
                              "}" +
                          "}";

            var tree = CSharpSyntaxTree.ParseText(code);
            int position = code.IndexOf("int b");

            // act
            MethodDeclarationSyntax method = tree.GetRoot().GetMethodAt(position);

            // assert
            Assert.IsNotNull(method);
            Assert.That(method.Identifier.ToString(), Is.EqualTo("Test2"));
        }
    }
}