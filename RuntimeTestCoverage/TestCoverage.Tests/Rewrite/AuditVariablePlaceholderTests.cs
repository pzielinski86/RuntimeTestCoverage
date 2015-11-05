using NUnit.Framework;
using TestCoverage.Rewrite;

namespace TestCoverage.Tests.Rewrite
{
    [TestFixture]
    public class AuditVariablePlaceholderTests
    {

        [Test]
        public void GenerateInitAuditVariableCode_Should_CreateValidCode()
        {
            const string expectedSourceCode =
                "new AuditVariable(){NodePath=\"A.B.C.D\",DocumentPath=@\"HelloWorld.cs\",Span=123}";

            var variable = new AuditVariablePlaceholder("HelloWorld.cs", "A.B.C.D", 123);

            string classSourceCode = variable.ToString();

            Assert.That(classSourceCode, Is.EqualTo(expectedSourceCode));
        }

    }
}