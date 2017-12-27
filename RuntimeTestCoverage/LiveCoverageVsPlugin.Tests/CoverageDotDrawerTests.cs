using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using TestCoverage.CoverageCalculation;

namespace LiveCoverageVsPlugin.Tests
{
    [TestFixture]
    public class CoverageDotDrawerTests
    {
        private List<LineCoverage> _linesCoverage;
        private const string DocumentName = "Tests";
        const string ProjectName = "TestProject";

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

            var sut = new CoverageDotDrawer(_linesCoverage, sourceCode, DocumentName);

            // act
            IEnumerable<CoverageDot> dots = sut.Draw(GetLineStartPositions(sourceCode), true,ProjectName);

            // assert
            Assert.That(dots.Count(), Is.EqualTo(0));
        }

        [Test]
        public void Should_DrawGreenDot_When_LineIsCovered_And_AssertionDidNotFail()
        {
            // arrange
            const string sourceCode = @"class Test
                                        {
	                                        public void TestMethod()
	                                        {
		                                        int a=0;
	                                        }
                                        }";

            _linesCoverage.Add(new LineCoverage());
            _linesCoverage[0].IsSuccess = true;
            _linesCoverage[0].NodePath = $"{ProjectName}.{DocumentName}.Test.TestMethod";
            _linesCoverage[0].Span = sourceCode.IndexOf("int a=0;", StringComparison.Ordinal) -
                                     sourceCode.IndexOf("public void", StringComparison.Ordinal);

            var sut = new CoverageDotDrawer(_linesCoverage, sourceCode, DocumentName);

            // act
            CoverageDot[] dots = sut.Draw(GetLineStartPositions(sourceCode), false, ProjectName).ToArray();

            // assert
            Assert.That(dots.Length, Is.EqualTo(1));
            Assert.That(dots.First().Color, Is.EqualTo(Brushes.Green));
        }

        [Test]
        public void Should_DrawRedDot_When_LineIsCovered_And_AssertionFailed()
        {
            // arrange
            const string sourceCode = @"class Test
                                        {
	                                        public void TestMethod()
	                                        {
                                                Assert.IsTrue(false);
	                                        }
                                        }";

            _linesCoverage.Add(new LineCoverage());
            _linesCoverage[0].IsSuccess = false;
            _linesCoverage[0].NodePath = $"{ProjectName}.{DocumentName}.Test.TestMethod";
            _linesCoverage[0].Span = sourceCode.IndexOf("Assert.IsTrue(false)", StringComparison.Ordinal) -
                sourceCode.IndexOf("public void", StringComparison.Ordinal);

            var sut = new CoverageDotDrawer(_linesCoverage, sourceCode, DocumentName);

            // act
            CoverageDot[] dots = sut.Draw(GetLineStartPositions(sourceCode), false, ProjectName).ToArray();

            // assert
            Assert.That(dots.Length, Is.EqualTo(1));
            Assert.That(dots.First().Color, Is.EqualTo(Brushes.Red));
        }

        [Test]
        public void Should_DrawGrayDot_When_LineIsCovered_And_CalcsAreInProgress()
        {
            // arrange
            const string sourceCode = @"class Test
                                        {
	                                        public vodi TestMethod()
	                                        {
                                                Assert.IsTrue(false);
	                                        }
                                        }";

            _linesCoverage.Add(new LineCoverage());
            _linesCoverage[0].IsSuccess = false;
            _linesCoverage[0].NodePath = $"{ProjectName}.{DocumentName}.Test.TestMethod";
            _linesCoverage[0].Span = sourceCode.IndexOf("Assert.IsTrue(false)", StringComparison.Ordinal);

            var sut = new CoverageDotDrawer(_linesCoverage, sourceCode, DocumentName);

            // act
            CoverageDot[] dots = sut.Draw(GetLineStartPositions(sourceCode), true, ProjectName).ToArray();

            // assert
            Assert.That(dots.Length, Is.EqualTo(1));
            Assert.That(dots.First().Color, Is.EqualTo(Brushes.DarkGray));
        }

        [Test]
        public void Should_DrawSilveryDot_When_LineIsNotCovered_And_CalsAreNotInProgress()
        {
            // arrange
            const string sourceCode = @"class Test
                                        {
	                                        public vodi TestMethod()
	                                        {
                                                Assert.IsTrue(false);
	                                        }
                                        }";

            var sut = new CoverageDotDrawer(_linesCoverage, sourceCode, DocumentName);

            // act
            CoverageDot[] dots = sut.Draw(GetLineStartPositions(sourceCode), false, ProjectName).ToArray();

            // assert
            Assert.That(dots.Length, Is.EqualTo(1));
            Assert.That(dots.First().Color, Is.EqualTo(Brushes.Silver));
        }

        [Test]
        public void ShouldNot_DrawDot_When_LineIsCovered_But_TheCodeIsBeforeDrawRange()
        {
            // arrange
            const string sourceCode = @"class Test
                                        {
	                                        public void TestMethod()
	                                        {
		                                        int a=0;
	                                        }
                                        }";

            _linesCoverage.Add(new LineCoverage());
            _linesCoverage[0].IsSuccess = true;
            _linesCoverage[0].NodePath = $"{ProjectName}.{DocumentName}.Test.TestMethod";
            _linesCoverage[0].Span = sourceCode.IndexOf("int a=0;", StringComparison.Ordinal);

            var sut = new CoverageDotDrawer(_linesCoverage, sourceCode, DocumentName);
            var lineStartPositions = GetLineStartPositions(sourceCode);

            // act
            CoverageDot[] dots = sut.Draw(lineStartPositions.Skip(5).ToArray(), false, ProjectName).ToArray();

            // assert
            Assert.That(dots.Length, Is.EqualTo(0));
        }

        [Test]
        public void ShouldNot_DrawDot_When_LineIsCovered_But_TheCodeIsAfterDrawRange()
        {
            // arrange
            const string sourceCode = @"class Test
                                        {
	                                        public void TestMethod()
	                                        {
		                                        int a=0;
	                                        }
                                        }";

            _linesCoverage.Add(new LineCoverage());
            _linesCoverage[0].IsSuccess = true;
            _linesCoverage[0].NodePath = $"{ProjectName}.{DocumentName}.Test.TestMethod";
            _linesCoverage[0].Span = sourceCode.IndexOf("int a=0;", StringComparison.Ordinal);

            var sut = new CoverageDotDrawer(_linesCoverage, sourceCode, DocumentName);
            var lineStartPositions = GetLineStartPositions(sourceCode);

            // act
            CoverageDot[] dots = sut.Draw(lineStartPositions.Take(4).ToArray(), false, ProjectName).ToArray();

            // assert
            Assert.That(dots.Length, Is.EqualTo(0));
        }

        [Test]
        public void Should_DrawDots_ForAllMethods()
        {
            // arrange
            const string sourceCode = @"class Test
                                        {
	                                        public void TestMethod()
	                                        {
		                                        int a=0;
	                                        }
                                            
                                            public void TestMethod2()
	                                        {
		                                        int a=0;
                                                int b=0;
	                                        }
                                        }";


            var sut = new CoverageDotDrawer(_linesCoverage, sourceCode, DocumentName);

            // act
            CoverageDot[] dots = sut.Draw(GetLineStartPositions(sourceCode), false, ProjectName).ToArray();

            // assert
            Assert.That(dots.Length, Is.EqualTo(3));
        }

        [Test]
        public void Should_DrawDot_BetweenLeadingTrivia_And_Statement()
        {
            // arrange
            const string sourceCode = @"class Test
                                        {
	                                        public void TestMethod()
	                                        {
                                                // leading trivia
                                                    int a=32;// line number 5
	                                        }
                                        }";

            _linesCoverage.Add(new LineCoverage());
            _linesCoverage[0].IsSuccess = true;
            _linesCoverage[0].NodePath = $"{ProjectName}.{DocumentName}.Test.TestMethod";
            _linesCoverage[0].Span = sourceCode.IndexOf("int a=32", StringComparison.Ordinal)-
                      sourceCode.IndexOf("public void", StringComparison.Ordinal);

            var sut = new CoverageDotDrawer(_linesCoverage, sourceCode,DocumentName);

            // act
            CoverageDot[] dots = sut.Draw(GetLineStartPositions(sourceCode), false, ProjectName).ToArray();
            
            // assert
            Assert.That(dots.Length, Is.EqualTo(1));
            Assert.That(dots.First().LineNumber, Is.EqualTo(5));
            Assert.That(dots.First().Color, Is.EqualTo(Brushes.Green));
        }

        private int[] GetLineStartPositions(string text)
        {
            string[] lines = text.Split('\n');
            int[] positions = new int[lines.Length];
            int previousPos = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                positions[i] = text.IndexOf(lines[i], previousPos, StringComparison.Ordinal);
                previousPos = positions[i];
            }

            return positions;
        }
    }
}
