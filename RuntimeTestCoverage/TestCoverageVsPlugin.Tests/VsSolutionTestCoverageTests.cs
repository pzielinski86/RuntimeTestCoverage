using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TestCoverage.CoverageCalculation;

namespace TestCoverageVsPlugin.Tests
{
    [TestFixture]
    public class VsSolutionTestCoverageTests
    {
        private List<LineCoverage> _linesCoverage;

        [SetUp]
        public void Setup()
        {
            _linesCoverage = new List<LineCoverage>();
        }

        [Test]
        public void ShouldNot_DrawAnyDots_When_ThereAreNoMethodsInCode()
        {
            // arrange
            const string sourceCode = "using System;\nclass Test{\n}";
            string[] lines = sourceCode.Split('\n');
            var sut = new CoverageDotDrawer(_linesCoverage, sourceCode, 0);

            // act
            IEnumerable<CoverageDot> dots = sut.Draw(lines, true);

            // assert
            Assert.That(dots.Count(), Is.EqualTo(0));
        }

        [Test]
        public void Should_DrawGreenDot_When_LineIsCovered_And_AssertionDidNotFail()
        {
            // arrange
            const string sourceCode = @"class Test
                                        {
	                                        public vodi TestMethod()
	                                        {
		                                        int a=0;
	                                        }
                                        }";

            _linesCoverage.Add(new LineCoverage());
            _linesCoverage[0].IsSuccess = true;
            _linesCoverage[0].Span = sourceCode.IndexOf("int a=0;", StringComparison.Ordinal);

            string[] lines = sourceCode.Split('\n');
            var sut = new CoverageDotDrawer(_linesCoverage, sourceCode, 0);
            
            // act
            CoverageDot[] dots = sut.Draw(lines, false).ToArray();

            // assert
            Assert.That(dots.Length, Is.EqualTo(1));
            Assert.That(dots.First().Color, Is.EqualTo(Brushes.Green));
        }
    }
}
