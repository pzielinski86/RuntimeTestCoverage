using System.Collections.Generic;
using NUnit.Framework;
using TestCoverage.CoverageCalculation;
using TestCoverageVsPlugin.Extensions;

namespace TestCoverageVsPlugin.Tests.Extensions
{
    [TestFixture]
    public class SolutionCoverageByDocumentExtensionsTests
    {
        [Test]
        public void GetTestPaths_Should_ReturnTestPaths_ForSutCode()
        {
            // arrange
            var coverage = new Dictionary<string, List<LineCoverage>>
            {
                ["Tests1.cs"] = new List<LineCoverage>(),
                ["Tests2.cs"] = new List<LineCoverage>(),
                ["Sut1.cs"] = new List<LineCoverage>(),
                ["Sut2.cs"] = new List<LineCoverage>()
            };


            coverage["Tests1.cs"].Add(new LineCoverage { TestPath = "Tests1.Path", NodePath = "Tests1.Path" });
            coverage["Tests2.cs"].Add(new LineCoverage { TestPath = "Tests2.Path", NodePath = "Tests2.Path" });

            coverage["Sut1.cs"].Add(new LineCoverage { TestPath = "Tests1.Path", NodePath = "Sut1.Path" });
            coverage["Sut2.cs"].Add(new LineCoverage { TestPath = "Tests1.Path", NodePath = "Sut2.Path" });

            // act
            string[] testPaths = coverage.GetTestPaths("Sut1.Path");

            // assert
            Assert.That(testPaths.Length, Is.EqualTo(1));
            Assert.That(testPaths[0], Is.EqualTo("Tests1.Path"));

        }

        [Test]
        public void MergeByNodePath_ShouldReplace_With_NewCoverage()
        {
            // arrange
            var coverage = new Dictionary<string, List<LineCoverage>>
            {
                ["Sut.cs"] = new List<LineCoverage>()
            };

            coverage["Sut.cs"].Add(new LineCoverage { NodePath = "path1",TestPath = "TestPath1"});

            var newCoverage = new List<LineCoverage>();
            newCoverage.Add(new LineCoverage { NodePath = "path1", TestPath = "TestPath1", DocumentPath = "Sut.cs" });

            // act
            coverage.MergeByNodePath(newCoverage);

            // assert
            Assert.That(coverage.Count, Is.EqualTo(1));
            Assert.That(coverage["Sut.cs"].Count, Is.EqualTo(1));
            Assert.That(coverage["Sut.cs"][0], Is.EqualTo(newCoverage[0]));
        }

        [Test]
        public void MergeByNodePath_ShouldNotReplace_With_NewCoverage_When_NewCoverageComeFromDifferentTest()
        {
            // arrange
            var coverage = new Dictionary<string, List<LineCoverage>>
            {
                ["Sut.cs"] = new List<LineCoverage>()
            };

            coverage["Sut.cs"].Add(new LineCoverage { NodePath = "path1", TestPath = "Test1.cs" });

            var newCoverage = new List<LineCoverage>();
            newCoverage.Add(new LineCoverage { NodePath = "path1", DocumentPath = "Sut.cs", TestPath = "Test2.cs" });

            // act
            coverage.MergeByNodePath(newCoverage);

            // assert
            Assert.That(coverage.Count, Is.EqualTo(1));
            Assert.That(coverage["Sut.cs"].Count, Is.EqualTo(2));
            Assert.That(coverage["Sut.cs"][0], Is.EqualTo(coverage["Sut.cs"][0]));
            Assert.That(coverage["Sut.cs"][1], Is.EqualTo(newCoverage[0]));
        }
    }
}