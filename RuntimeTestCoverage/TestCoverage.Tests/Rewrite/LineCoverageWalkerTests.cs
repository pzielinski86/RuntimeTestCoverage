using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestCoverage.Rewrite;

namespace TestCoverage.Tests.Rewrite
{
    [TestFixture]
    public class LineCoverageWalkerTests
    {
        [Test]
        public void ChangeIt()
        {
            const string projectName="ProjectName";
            const string documentPath="c:\\doc.cs";

            LineCoverageWalker walker = new LineCoverageWalker(projectName, documentPath);
        }
    }
}
